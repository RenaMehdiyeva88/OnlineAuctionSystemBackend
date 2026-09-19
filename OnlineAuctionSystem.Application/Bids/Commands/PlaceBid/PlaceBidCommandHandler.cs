using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.Contracts.Bids;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Application.Bids.Commands.PlaceBid
{
    public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, BidDto>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTime _dateTime;
        private readonly IMapper _mapper;

        public PlaceBidCommandHandler(
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            IDateTime dateTime,
            IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _mapper = mapper;
        }

        public async Task<BidDto> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
        {
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken)
                ?? throw new NotFoundException(nameof(Auction), request.AuctionId);

            // Uses the injected IDateTime, NOT auction.IsExpired (which reads
            // DateTime.UtcNow directly) — a handler must never read the real
            // system clock itself, or it becomes untestable: unit tests fix
            // "now" via a mocked IDateTime, and an entity that ignores that
            // mock and reads the real clock will always disagree with
            // whatever fake EndTime the test set up.
            if (auction.Status != AuctionStatus.Active || auction.EndTime <= _dateTime.UtcNow)
                throw new AuctionClosedException(auction.Id);

            var bidder = await _userRepository.GetByIdAsync(request.BidderId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.BidderId);

            if (auction.SellerId == bidder.Id)
                throw new ForbiddenException("Sellers cannot bid on their own auctions.");

            var previousTopBid = await _bidRepository.GetHighestBidAsync(auction.Id, cancellationToken);

            // The very first bid on an auction is allowed to equal the starting
            // price exactly — only subsequent bids must strictly exceed the
            // current highest. The old code compared against
            // (previousTopBid?.Amount ?? auction.StartingPrice) with a plain
            // "<=" check either way, which silently rejected a first bid equal
            // to the starting price (e.g. StartingPrice=100, first bid of 100
            // failed "100 <= 100").
            var isFirstBid = previousTopBid is null;

            if (isFirstBid)
            {
                if (request.Amount < auction.StartingPrice)
                    throw new InvalidBidException($"First bid must be at least the starting price ({auction.StartingPrice}).");
            }
            else if (request.Amount < previousTopBid!.Amount + auction.MinimumIncrement)
            {
                // Minimum increment: a bid of 100.01 over a 100.00 highest bid
                // used to be accepted, which lets an auction be "won" by a
                // single cent — most real auction sites require a meaningful
                // minimum raise (auction.MinimumIncrement, default 1.00).
                throw new InvalidBidException(
                    $"Bid must be at least {auction.MinimumIncrement} higher than the current highest bid ({previousTopBid!.Amount}).");
            }

            var bid = new Bid
            {
                AuctionId = auction.Id,
                BidderId = bidder.Id,
                Amount = request.Amount
            };

            await _bidRepository.AddAsync(bid, cancellationToken);

            // Touch the auction row so EF Core includes it in the UPDATE and
            // checks RowVersion. If another request placed a bid on this same
            // auction between our read (GetHighestBidAsync above) and this
            // save, the RowVersion will have changed underneath us and EF
            // throws DbUpdateConcurrencyException below — that's exactly the
            // race condition we're guarding against (two lower/higher bids
            // both reading the same "current highest" and both succeeding).
            auction.UpdatedAt = _dateTime.UtcNow;
            _auctionRepository.Update(auction);

            if (previousTopBid is not null && previousTopBid.BidderId != bidder.Id)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = previousTopBid.BidderId,
                    AuctionId = auction.Id,
                    Message = $"You've been outbid on \"{auction.Title}\". New highest bid: {request.Amount}."
                }, cancellationToken);
            }

            try
            {
                // Single SaveChangesAsync for the whole use case — the bid,
                // the auction's concurrency-token touch, and the outbid
                // notification all commit together in one transaction.
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "Someone just placed a bid on this auction before yours went through. Please refresh and try again.");
            }

            if (previousTopBid is not null && previousTopBid.BidderId != bidder.Id)
            {
                await _notificationService.NotifyOutbidAsync(
                    previousTopBid.BidderId, auction.Id, request.Amount, cancellationToken);
            }

            // Notify seller of new bid on their auction
            await _notificationService.NotifyNewBidAsync(
                auction.SellerId, auction.Id, request.Amount, bidder.Username, cancellationToken);

            await _notificationService.NotifyBidPlacedAsync(
                auction.Id, request.Amount, bidder.Username, cancellationToken);

            bid.Bidder = bidder;
            return _mapper.Map<BidDto>(bid);
        }
    }
}
