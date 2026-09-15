using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using OnlineAuctionSystem.Persistence.Context;

namespace OnlineAuctionSystem.Persistence.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AuctionDbContext _context;

        public AuctionRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Seller)
                .Include(a => a.Category)
                .Include(a => a.Winner)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        // F8 — keyword + category + price range filter over active auctions.
        public async Task<(List<Auction> Items, int TotalCount)> SearchAsync(
            string? keyword,
            Guid? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Auctions
                .Include(a => a.Seller)
                .Include(a => a.Category)
                .Include(a => a.Bids)
                // EndTime > UtcNow closes a ~30s window: AuctionAutoCloseService only
                // runs every 30 seconds, so an auction can sit "Active" in the DB for
                // up to that long after it actually expired. Without this extra check,
                // search results could show an already-expired lot as biddable.
                .Where(a => a.Status == AuctionStatus.Active && a.EndTime > DateTime.UtcNow)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(a => a.Title.Contains(keyword) || a.Description.Contains(keyword));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            // Filter by the auction's ACTUAL current price (highest bid so far,
            // or the starting price if nobody has bid yet) — not just
            // StartingPrice. Filtering on StartingPrice alone let a lot that
            // started at $50 but is currently at $800 show up under a "under
            // $100" search, which is misleading to buyers.
            if (minPrice.HasValue)
            {
                query = query.Where(a =>
                    (a.Bids.Any() ? a.Bids.Max(b => b.Amount) : a.StartingPrice) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(a =>
                    (a.Bids.Any() ? a.Bids.Max(b => b.Amount) : a.StartingPrice) <= maxPrice.Value);
            }

            // TotalCount is computed against the SAME filtered query, before
            // paging — the frontend previously had no way to know how many
            // lots existed in total (only ever got back one page's worth of
            // items), so it couldn't build a real pagination control.
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(a => a.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        // F6 — seller dashboard: active and completed auctions.
        // No .Include(a => a.Bids) here on purpose — GetSellerDashboardQueryHandler
        // pairs this with GetBidStatsForAuctionsAsync (a SQL GROUP BY) instead
        // of materializing every bid row just to Count()/Max() them in memory.
        public async Task<List<Auction>> GetBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Category)
                .Include(a => a.Winner)
                .Where(a => a.SellerId == sellerId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);

        // Computes bid count + highest amount per auction directly in SQL
        // (GROUP BY ... COUNT(*) ... MAX(Amount)) — no Bid entities are
        // loaded into memory, unlike the old .Include(a => a.Bids) approach.
        public async Task<Dictionary<Guid, (int TotalBids, decimal? HighestBid)>> GetBidStatsForAuctionsAsync(
            IEnumerable<Guid> auctionIds, CancellationToken cancellationToken = default)
        {
            var ids = auctionIds.ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, (int, decimal?)>();

            var stats = await _context.Bids
                .Where(b => ids.Contains(b.AuctionId))
                .GroupBy(b => b.AuctionId)
                .Select(g => new
                {
                    AuctionId = g.Key,
                    TotalBids = g.Count(),
                    HighestBid = g.Max(b => b.Amount)
                })
                .ToListAsync(cancellationToken);

            return stats.ToDictionary(s => s.AuctionId, s => (s.TotalBids, (decimal?)s.HighestBid));
        }

        // F4 — used by the background job to find auctions that need to be auto-closed.
        public async Task<List<Auction>> GetExpiredActiveAuctionsAsync(CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Bids)
                .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Auction auction, CancellationToken cancellationToken = default) =>
            await _context.Auctions.AddAsync(auction, cancellationToken);

        public void Update(Auction auction) => _context.Auctions.Update(auction);
    }
}