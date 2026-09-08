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

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
            await _context.Notifications.AddAsync(notification, cancellationToken);
    }
}
