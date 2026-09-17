using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction
{
    public record CreateAuctionCommand(
        string Title,
        string Description,
        string? ImageUrl,
        decimal StartingPrice,
        DateTime EndTime,
        Guid CategoryId,
        Guid SellerId,
        decimal MinimumIncrement
    ) : IRequest<AuctionDto>;
}
