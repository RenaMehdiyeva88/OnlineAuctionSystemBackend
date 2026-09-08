namespace OnlineAuctionSystem.Contracts.Auctions
{
    // Used for F8: category-based browsing and search with price range filter
    public record AuctionSearchRequest(
        string? Keyword,
        Guid? CategoryId,
        decimal? MinPrice,
        decimal? MaxPrice,
        int Page = 1,
        int PageSize = 20
    );
}
