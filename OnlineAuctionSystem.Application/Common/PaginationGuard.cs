namespace OnlineAuctionSystem.Application.Common
{
    public static class PaginationGuard
    {
        public static (int Page, int PageSize) Clamp(int page, int pageSize) =>
            (Math.Max(1, page), Math.Clamp(pageSize, 1, 100));
    }
}