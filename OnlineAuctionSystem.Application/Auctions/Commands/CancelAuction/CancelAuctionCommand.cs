using MediatR;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CancelAuction
{
    // Only allowed while the auction has zero bids — enforced in the handler.
    public record CancelAuctionCommand(Guid AuctionId, Guid CurrentUserId) : IRequest<Unit>;
}
