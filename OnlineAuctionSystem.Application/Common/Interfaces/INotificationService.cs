namespace OnlineAuctionSystem.Application.Common.Interfaces.Services
{
    public interface INotificationService
    {
        Task NotifyOutbidAsync(Guid userId, Guid auctionId, decimal newHighestBid, CancellationToken cancellationToken = default);
        Task NotifyNewBidAsync(Guid sellerId, Guid auctionId, decimal newBidAmount, string bidderName, CancellationToken cancellationToken = default);
        Task NotifyAuctionWonAsync(Guid winnerId, Guid auctionId, decimal winningAmount, CancellationToken cancellationToken = default);
        Task NotifyAuctionClosedAsync(Guid sellerId, Guid auctionId, CancellationToken cancellationToken = default);
        Task NotifyBidPlacedAsync(Guid auctionId, decimal newAmount, string bidderName, CancellationToken cancellationToken = default);
    }
}