using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Contracts.Common;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionsByCategory
{

    // F8: Category-based browsing and search with price range filter.
    // Returns PagedResult now (not a bare List) — same pagination shape as
    // bid history / notifications, so the frontend can render page controls
    // and knows the real total instead of just "however many came back".
    public record GetAuctionsByCategoryQuery(
        string? Keyword,
        Guid? CategoryId,
        decimal? MinPrice,
        decimal? MaxPrice,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<AuctionListItemDto>>;
}