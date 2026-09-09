using MediatR;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CloseAuction
{
    // F4/F5: auto-close on expiry + winner determination and notification
    // CurrentUserId is the seller requesting the close, taken from their JWT
    // (never trust a client-supplied "sellerId" — that's how the IDOR bug
    // happened). Only manual closes go through this command/handler; the
    // background AuctionAutoCloseService has its own logic and doesn't need
    // an owner to check against.
    public record CloseAuctionCommand(Guid AuctionId, Guid CurrentUserId) : IRequest<Unit>;
}
