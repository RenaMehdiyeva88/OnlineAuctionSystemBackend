namespace OnlineAuctionSystem.Contracts.Auctions
{
    public record CreateAuctionRequest(
    string Title,
    string Description,
    string? ImageUrl,
    decimal StartingPrice,
    DateTime EndTime,
    Guid CategoryId
);
}
