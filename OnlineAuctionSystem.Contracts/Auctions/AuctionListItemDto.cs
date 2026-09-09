namespace OnlineAuctionSystem.Contracts.Auctions
{
    // Lightweight projection used for browsing/search (F8)
    public record AuctionListItemDto(
        Guid Id,
        string Title,
        string? ImageUrl,
        decimal StartingPrice,
        decimal CurrentHighestBid,
        DateTime EndTime,
        string Status,
        Guid SellerId,
        string CategoryName
    );
}
