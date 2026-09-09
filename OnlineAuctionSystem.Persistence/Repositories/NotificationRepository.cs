using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Persistence.Context;

namespace OnlineAuctionSystem.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AuctionDbContext _context;

        public NotificationRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetByUserIdAsync(
            Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
            await _context.Notifications.AddAsync(notification, cancellationToken);

        public void Update(Notification notification) => _context.Notifications.Update(notification);
    }
}
