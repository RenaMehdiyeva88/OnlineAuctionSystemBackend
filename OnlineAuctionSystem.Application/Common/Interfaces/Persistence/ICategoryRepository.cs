using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{

    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
