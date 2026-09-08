using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CloseAuction
{
    public class CloseAuctionCommandHandler : IRequestHandler<CloseAuctionCommand, Unit>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        public CloseAuctionCommandHandler(
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            INotificationRepository notificationRepository,
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(CloseAuctionCommand request, CancellationToken cancellationToken)
        {
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken)
                ?? throw new NotFoundException(nameof(Auction), request.AuctionId);

            if (auction.Status != AuctionStatus.Active)
                return Unit.Value; // already closed, nothing to do

            auction.Status = AuctionStatus.Closed;

            var winningBid = await _bidRepository.GetHighestBidAsync(auction.Id, cancellationToken);
            if (winningBid is not null)
            {
                auction.WinnerId = winningBid.BidderId;

                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = winningBid.BidderId,
                    AuctionId = auction.Id,
                    Message = $"Congratulations! You won the auction \"{auction.Title}\" with a bid of {winningBid.Amount}."
                }, cancellationToken);

                await _notificationService.NotifyAuctionWonAsync(winningBid.BidderId, auction.Id, winningBid.Amount, cancellationToken);
            }

            await _notificationRepository.AddAsync(new Notification
            {
                UserId = auction.SellerId,
                AuctionId = auction.Id,
                Message = $"Your auction \"{auction.Title}\" has closed."
            }, cancellationToken);
            await _notificationService.NotifyAuctionClosedAsync(auction.SellerId, auction.Id, cancellationToken);

            _auctionRepository.Update(auction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}