using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.Application.Common;
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

        public async Task<(List<Bid> Items, int TotalCount)> GetByAuctionIdAsync(
            Guid auctionId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = PaginationGuard.Clamp(pageNumber, pageSize);

            var query = _context.Bids
                .Include(b => b.Bidder)
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken cancellationToken = default) =>
            await _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task AddAsync(Bid bid, CancellationToken cancellationToken = default) =>
            await _context.Bids.AddAsync(bid, cancellationToken);
    }
}