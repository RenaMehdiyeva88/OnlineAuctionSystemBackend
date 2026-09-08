using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Bids.DTOs;
using OnlineAuctionSystem.Application.Common.Exceptions;
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
        private readonly IMapper _mapper;

        public PlaceBidCommandHandler(
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BidDto> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
        {
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken)
                ?? throw new NotFoundException(nameof(Auction), request.AuctionId);

            if (auction.Status != AuctionStatus.Active || auction.IsExpired)
                throw new AuctionClosedException(auction.Id);

            var bidder = await _userRepository.GetByIdAsync(request.BidderId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.BidderId);

            if (auction.SellerId == bidder.Id)
                throw new ForbiddenException("Sellers cannot bid on their own auctions.");

            var previousTopBid = await _bidRepository.GetHighestBidAsync(auction.Id, cancellationToken);
            var currentHighest = previousTopBid?.Amount ?? auction.StartingPrice;

            if (request.Amount <= currentHighest)
                throw new InvalidBidException($"Bid must be higher than the current highest bid ({currentHighest}).");

            var bid = new Bid
            {
                AuctionId = auction.Id,
                BidderId = bidder.Id,
                Amount = request.Amount
            };

            await _bidRepository.AddAsync(bid, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (previousTopBid is not null && previousTopBid.BidderId != bidder.Id)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = previousTopBid.BidderId,
                    AuctionId = auction.Id,
                    Message = $"You've been outbid on \"{auction.Title}\". New highest bid: {request.Amount}."
                }, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _notificationService.NotifyOutbidAsync(
                    previousTopBid.BidderId, auction.Id, request.Amount, cancellationToken);
            }

            // Notify seller of new bid on their auction
            await _notificationService.NotifyNewBidAsync(
                auction.SellerId, auction.Id, request.Amount, bidder.Username, cancellationToken);

            bid.Bidder = bidder;
            return _mapper.Map<BidDto>(bid);
        }
    }
}
