using Microsoft.AspNetCore.SignalR;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;

namespace OnlineAuctionSystem.Infrastructure.Notifications
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyOutbidAsync(Guid userId, Guid auctionId, decimal newHighestBid, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("OutBid", new { auctionId, newHighestBid }, cancellationToken);
        }

        public async Task NotifyNewBidAsync(Guid sellerId, Guid auctionId, decimal newBidAmount, string bidderName, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(sellerId.ToString()).SendAsync("NewBid", new { auctionId, newBidAmount, bidderName }, cancellationToken);
        }

        public async Task NotifyAuctionWonAsync(Guid winnerId, Guid auctionId, decimal winningAmount, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(winnerId.ToString()).SendAsync("AuctionWon", new { auctionId, winningAmount }, cancellationToken);
        }

        public async Task NotifyAuctionClosedAsync(Guid sellerId, Guid auctionId, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(sellerId.ToString()).SendAsync("AuctionClosed", new { auctionId }, cancellationToken);
        }

        public async Task NotifyBidPlacedAsync(Guid auctionId, decimal newAmount, string bidderName, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group($"auction-{auctionId}").SendAsync("BidPlaced", new { auctionId, newAmount, bidderName }, cancellationToken);
        }
    }
}