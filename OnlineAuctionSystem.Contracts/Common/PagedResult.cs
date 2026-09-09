namespace OnlineAuctionSystem.Contracts.Common
{
    // Generic pagination envelope used anywhere a list could grow large
    // enough to need paging (bid history, notifications, etc.).
    public record PagedResult<T>(
        List<T> Items,
        int PageNumber,
        int PageSize,
        int TotalCount
    )
    {
        public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
