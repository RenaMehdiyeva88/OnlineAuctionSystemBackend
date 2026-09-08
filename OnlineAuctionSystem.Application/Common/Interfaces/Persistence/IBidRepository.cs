using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{

    public interface IBidRepository
    {
        // F7 — full bid history for an auction, newest first.
        Task<List<Bid>> GetByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default);

        // F3 — used to validate a new bid against the current highest.
        Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken cancellationToken = default);

        Task AddAsync(Bid bid, CancellationToken cancellationToken = default);
    }
}
