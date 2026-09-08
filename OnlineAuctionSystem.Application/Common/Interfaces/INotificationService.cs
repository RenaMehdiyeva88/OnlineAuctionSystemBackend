namespace OnlineAuctionSystem.Application.Common.Interfaces.Services
{
    // Real-time push (implemented via SignalR in Presentation/Infrastructure).
    // Separate from INotificationRepository, which persists the notification
    // record used by GetNotificationsQuery.
    public interface INotificationService
    {
        Task NotifyOutbidAsync(Guid userId, Guid auctionId, decimal newHighestBid, CancellationToken cancellationToken = default);
        Task NotifyNewBidAsync(Guid sellerId, Guid auctionId, decimal newBidAmount, string bidderName, CancellationToken cancellationToken = default);
        Task NotifyAuctionWonAsync(Guid winnerId, Guid auctionId, decimal winningAmount, CancellationToken cancellationToken = default);
        Task NotifyAuctionClosedAsync(Guid sellerId, Guid auctionId, CancellationToken cancellationToken = default);
    }
}
