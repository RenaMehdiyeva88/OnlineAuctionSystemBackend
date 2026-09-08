using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Persistence.Context;

namespace OnlineAuctionSystem.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AuctionDbContext _context;

        public CategoryRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
