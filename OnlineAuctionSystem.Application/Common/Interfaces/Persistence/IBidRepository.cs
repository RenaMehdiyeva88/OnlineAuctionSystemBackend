using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{

    public interface IBidRepository
    {
        // F7 — paginated bid history for an auction, newest first. Auctions
        // can accumulate thousands of bids near closing time, so this never
        // returns the full table.
        Task<(List<Bid> Items, int TotalCount)> GetByAuctionIdAsync(
            Guid auctionId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        // F3 — used to validate a new bid against the current highest.
        Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken cancellationToken = default);

        Task AddAsync(Bid bid, CancellationToken cancellationToken = default);
    }
}
