using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Persistence.Context;

namespace OnlineAuctionSystem.Persistence.Repositories
{

    public class BidRepository : IBidRepository
    {
        private readonly AuctionDbContext _context;

        public BidRepository(AuctionDbContext context)
        {
            _context = context;
        }

        // F7 — full bid history per auction, most recent first.
        public async Task<List<Bid>> GetByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default) =>
            await _context.Bids
                .Include(b => b.Bidder)
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(cancellationToken);

        // F3 — used to validate that a new bid beats the current highest.
        public async Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken cancellationToken = default) =>
            await _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task AddAsync(Bid bid, CancellationToken cancellationToken = default) =>
            await _context.Bids.AddAsync(bid, cancellationToken);
    }

}
