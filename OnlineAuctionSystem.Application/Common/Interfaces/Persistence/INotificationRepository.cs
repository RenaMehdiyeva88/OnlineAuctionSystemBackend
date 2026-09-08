using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    }
}
