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
        Guid SellerId,
        string SellerName,
        Guid CategoryId,
        string CategoryName,
        string? WinnerName
    );
}
