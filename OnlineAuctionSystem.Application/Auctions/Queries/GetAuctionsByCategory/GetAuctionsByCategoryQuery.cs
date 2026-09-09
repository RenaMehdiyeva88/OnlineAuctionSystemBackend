using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionsByCategory
{

    // F8: Category-based browsing and search with price range filter
    public record GetAuctionsByCategoryQuery(
        string? Keyword,
        Guid? CategoryId,
        decimal? MinPrice,
        decimal? MaxPrice,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<List<AuctionListItemDto>>;
}
