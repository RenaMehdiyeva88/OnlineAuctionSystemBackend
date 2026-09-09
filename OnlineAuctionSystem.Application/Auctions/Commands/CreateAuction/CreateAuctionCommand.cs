using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction
{

    // F2: Auction listing creation with title, description, starting price, and end time
    public record CreateAuctionCommand(
        string Title,
        string Description,
        string? ImageUrl,
        decimal StartingPrice,
        DateTime EndTime,
        Guid CategoryId,
        Guid SellerId
    ) : IRequest<AuctionDto>;
}
