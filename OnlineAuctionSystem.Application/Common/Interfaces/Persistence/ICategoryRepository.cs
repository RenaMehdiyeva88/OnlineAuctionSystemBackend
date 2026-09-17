using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task AddAsync(Category category, CancellationToken cancellationToken = default);
        void Update(Category category);

        // Soft delete only — never a hard DELETE, since existing auctions
        // may still reference this category and a real DELETE would either
        // violate the FK constraint or, worse, cascade-delete auction history.
        void SoftDelete(Category category);
    }
}
