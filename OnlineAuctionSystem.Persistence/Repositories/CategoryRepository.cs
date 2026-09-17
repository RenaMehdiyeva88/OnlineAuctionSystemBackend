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

        public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default) =>
            await _context.Categories.AnyAsync(
                c => c.Name.ToLower() == name.ToLower() && (excludeId == null || c.Id != excludeId), cancellationToken);

        public async Task AddAsync(Category category, CancellationToken cancellationToken = default) =>
            await _context.Categories.AddAsync(category, cancellationToken);

        public void Update(Category category) => _context.Categories.Update(category);

        public void SoftDelete(Category category)
        {
            category.IsDeleted = true;
            _context.Categories.Update(category);
        }
    }
}