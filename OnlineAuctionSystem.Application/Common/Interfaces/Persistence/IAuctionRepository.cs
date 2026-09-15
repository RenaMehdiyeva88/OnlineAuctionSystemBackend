using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{
    public interface IAuctionRepository
    {
        Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // F8 — category-based browsing/search with keyword + price range filter.
        // Returns TotalCount alongside the page of results (same pagination
        // pattern as IBidRepository/INotificationRepository) so the client
        // can build page-count UI instead of guessing how many lots exist.
        Task<(List<Auction> Items, int TotalCount)> SearchAsync(
            string? keyword,
            Guid? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        // F6 — seller dashboard: active and completed auctions for a given seller.
        // Deliberately does NOT include Bids — see GetBidStatsForAuctionsAsync,
        // which computes TotalBids/CurrentHighestBid via a SQL GROUP BY
        // instead of loading every bid row into memory just to Count()/Max()
        // them client-side.
        Task<List<Auction>> GetBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default);

        // Lightweight aggregate query for the seller dashboard: per-auction
        // bid count and highest bid amount, computed in SQL (COUNT/MAX),
        // without materializing Bid entities.
        Task<Dictionary<Guid, (int TotalBids, decimal? HighestBid)>> GetBidStatsForAuctionsAsync(
            IEnumerable<Guid> auctionIds, CancellationToken cancellationToken = default);

        // F4 — used by the background job to find auctions that need to be auto-closed.
        Task<List<Auction>> GetExpiredActiveAuctionsAsync(CancellationToken cancellationToken = default);

        Task AddAsync(Auction auction, CancellationToken cancellationToken = default);
        void Update(Auction auction);
    }
}