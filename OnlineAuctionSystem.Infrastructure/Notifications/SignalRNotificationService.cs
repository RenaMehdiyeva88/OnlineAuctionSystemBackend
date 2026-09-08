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

        // F3 — push to the outbid user's group the instant a higher bid lands.
        public async Task NotifyOutbidAsync(Guid userId, Guid auctionId, decimal newHighestBid, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(userId.ToString()).SendAsync(
                "OutBid",
                new { auctionId, newHighestBid },
                cancellationToken);
        }

        // F3 — push to the seller's group when a new bid is placed on their auction.
        public async Task NotifyNewBidAsync(Guid sellerId, Guid auctionId, decimal newBidAmount, string bidderName, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(sellerId.ToString()).SendAsync(
                "NewBid",
                new { auctionId, newBidAmount, bidderName },
                cancellationToken);
        }

        // F5 — notify the winner once the auction auto-closes.
        public async Task NotifyAuctionWonAsync(Guid winnerId, Guid auctionId, decimal winningAmount, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(winnerId.ToString()).SendAsync(
                "AuctionWon",
                new { auctionId, winningAmount },
                cancellationToken);
        }

        // F4/F5 — notify the seller that their auction has closed.
        public async Task NotifyAuctionClosedAsync(Guid sellerId, Guid auctionId, CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients.Group(sellerId.ToString()).SendAsync(
                "AuctionClosed",
                new { auctionId },
                cancellationToken);
        }
    }
}
