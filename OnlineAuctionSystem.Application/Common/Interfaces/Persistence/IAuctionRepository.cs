using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{
    public interface IAuctionRepository
    {
        Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // F7 — browse all active auctions (homepage/list view) with pagination.
        Task<List<Auction>> GetAllActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        // F8 — category-based browsing/search with keyword + price range filter.
        Task<List<Auction>> SearchAsync(
            string? keyword,
            Guid? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<List<Auction>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

        // F6 — seller dashboard: active and completed auctions for a given seller.
        Task<List<Auction>> GetBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default);

        // F4 — used by the background job to find auctions that need to be auto-closed.
        Task<List<Auction>> GetExpiredActiveAuctionsAsync(CancellationToken cancellationToken = default);

        Task AddAsync(Auction auction, CancellationToken cancellationToken = default);
        void Update(Auction auction);
    }
}
