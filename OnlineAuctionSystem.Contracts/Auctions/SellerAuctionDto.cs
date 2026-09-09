namespace OnlineAuctionSystem.Contracts.Auctions
{
    // Extended projection for the seller dashboard (F6) — includes fields
    // a seller cares about that a public buyer-facing list doesn't need.
    public record SellerAuctionDto(
        Guid Id,
        string Title,
        string? ImageUrl,
        decimal StartingPrice,
        decimal CurrentHighestBid,
        int TotalBids,
        DateTime EndTime,
        string Status,
        Guid? WinnerId,
        string? WinnerName
    );
}
