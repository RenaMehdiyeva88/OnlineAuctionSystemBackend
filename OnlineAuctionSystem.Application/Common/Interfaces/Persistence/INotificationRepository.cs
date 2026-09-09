using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{
    public interface INotificationRepository
    {
        // F3/F5 — paginated notification history for a user.
        Task<(List<Notification> Items, int TotalCount)> GetByUserIdAsync(
            Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
        void Update(Notification notification);
    }
}
