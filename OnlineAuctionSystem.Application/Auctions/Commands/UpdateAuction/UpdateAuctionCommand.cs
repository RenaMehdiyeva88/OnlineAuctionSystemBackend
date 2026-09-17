using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;

namespace OnlineAuctionSystem.Application.Auctions.Commands.UpdateAuction
{
    // Only allowed while the auction has zero bids — enforced in the handler.
    public record UpdateAuctionCommand(
        Guid AuctionId,
        Guid CurrentUserId,
        string Title,
        string Description,
        string? ImageUrl,
        decimal StartingPrice,
        DateTime EndTime,
        Guid CategoryId,
        decimal MinimumIncrement
    ) : IRequest<AuctionDto>;
}
