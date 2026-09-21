using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.Application.Common;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using OnlineAuctionSystem.Persistence.Context;

namespace OnlineAuctionSystem.Persistence.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AuctionDbContext _context;
        private readonly IDateTime _dateTime;

        public AuctionRepository(AuctionDbContext context, IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Seller)
                .Include(a => a.Category)
                .Include(a => a.Winner)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        public async Task<(List<Auction> Items, int TotalCount)> SearchAsync(
            string? keyword,
            Guid? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (page, pageSize) = PaginationGuard.Clamp(page, pageSize);

            var query = _context.Auctions
                .Include(a => a.Seller)
                .Include(a => a.Category)
                .Include(a => a.Bids)
                .Where(a => a.Status == AuctionStatus.Active && a.EndTime > _dateTime.UtcNow)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(a => a.Title.Contains(keyword) || a.Description.Contains(keyword));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

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

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(a => a.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<List<Auction>> GetBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Category)
                .Include(a => a.Winner)
                .Where(a => a.SellerId == sellerId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);

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

        public async Task<List<Auction>> GetExpiredActiveAuctionsAsync(CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Bids)
                .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= _dateTime.UtcNow)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Auction auction, CancellationToken cancellationToken = default) =>
            await _context.Auctions.AddAsync(auction, cancellationToken);

        public void Update(Auction auction) => _context.Auctions.Update(auction);
    }
}