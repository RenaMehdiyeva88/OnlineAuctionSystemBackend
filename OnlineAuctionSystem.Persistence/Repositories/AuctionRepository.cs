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
        public async Task<List<Auction>> SearchAsync(
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
                .Where(a => a.Status == AuctionStatus.Active)
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
                query = query.Where(a => a.StartingPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(a => a.StartingPrice <= maxPrice.Value);
            }

            return await query
                .OrderBy(a => a.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        // F8 — category-based browsing.
        public async Task<List<Auction>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Seller)
                .Include(a => a.Category)
                .Include(a => a.Bids)
                .Where(a => a.CategoryId == categoryId && a.Status == AuctionStatus.Active)
                .OrderBy(a => a.EndTime)
                .ToListAsync(cancellationToken);

        // F6 — seller dashboard: active and completed auctions.
        public async Task<List<Auction>> GetBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Category)
                .Include(a => a.Winner)
                .Include(a => a.Bids)
                .Where(a => a.SellerId == sellerId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);

        // F4 — used by the background job to find auctions that need to be auto-closed.
        public async Task<List<Auction>> GetExpiredActiveAuctionsAsync(CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Bids)
                .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Auction auction, CancellationToken cancellationToken = default) =>
            await _context.Auctions.AddAsync(auction, cancellationToken);

        // F8 — get all active auctions with pagination
        public async Task<List<Auction>> GetAllActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
            await _context.Auctions
                .Include(a => a.Seller)
                .Include(a => a.Category)
                .Include(a => a.Bids)
                .Where(a => a.Status == AuctionStatus.Active)
                .OrderBy(a => a.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        public void Update(Auction auction) => _context.Auctions.Update(auction);
    }
}
