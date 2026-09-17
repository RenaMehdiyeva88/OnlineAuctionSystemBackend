using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CancelAuction
{
    public class CancelAuctionCommandHandler : IRequestHandler<CancelAuctionCommand, Unit>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelAuctionCommandHandler(
            IAuctionRepository auctionRepository, IBidRepository bidRepository, IUnitOfWork unitOfWork)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(CancelAuctionCommand request, CancellationToken cancellationToken)
        {
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken)
                ?? throw new NotFoundException(nameof(Auction), request.AuctionId);

            if (auction.SellerId != request.CurrentUserId)
                throw new ForbiddenException("You can only cancel your own auctions.");

            if (auction.Status != AuctionStatus.Active)
                return Unit.Value; // already closed/cancelled — nothing to do

            var existingBid = await _bidRepository.GetHighestBidAsync(auction.Id, cancellationToken);
            if (existingBid is not null)
                throw new ForbiddenException("An auction that already has bids cannot be cancelled.");

            // AuctionStatus.Cancelled already existed on the enum but nothing
            // in the codebase ever set it — this is the only place that does.
            auction.Status = AuctionStatus.Cancelled;
            _auctionRepository.Update(auction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
