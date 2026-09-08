using MediatR;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CloseAuction
{
    // F4/F5: auto-close on expiry + winner determination and notification
    public record CloseAuctionCommand(Guid AuctionId) : IRequest<Unit>;
}
