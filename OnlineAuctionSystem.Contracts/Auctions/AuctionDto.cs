namespace OnlineAuctionSystem.Contracts.Auctions
{
    public record AuctionDto(
    Guid Id,
    string Title,
    string Description,
    string? ImageUrl,
    decimal StartingPrice,
    decimal CurrentHighestBid,
    DateTime EndTime,
    string Status,
    string SellerName,
    string CategoryName,
    string? WinnerName
);
}
