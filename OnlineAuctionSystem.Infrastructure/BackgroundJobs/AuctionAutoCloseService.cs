using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Infrastructure.BackgroundJobs
{
    // F4 — polls for auctions whose EndTime has passed and closes them,
    // F5 — determines the winner (highest bidder) and fires notifications.
    // Runs on a fixed interval inside a scoped service, since DbContext/repositories are scoped.
    public class AuctionAutoCloseService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuctionAutoCloseService> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

        public AuctionAutoCloseService(IServiceScopeFactory scopeFactory, ILogger<AuctionAutoCloseService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CloseExpiredAuctionsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while auto-closing expired auctions.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task CloseExpiredAuctionsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var auctionRepository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expiredAuctions = await auctionRepository.GetExpiredActiveAuctionsAsync(cancellationToken);

            // Collect who needs notifying as we go, but don't send anything
            // over SignalR until after SaveChangesAsync succeeds below — same
            // fix as CloseAuctionCommandHandler: don't tell clients "auction
            // closed, X won" if that write could still fail and roll back.
            var pendingNotifications = new List<(Guid UserId, Guid AuctionId, decimal? Amount, bool IsWinner)>();

            foreach (var auction in expiredAuctions)
            {
                auction.Status = AuctionStatus.Closed;

                var winningBid = auction.Bids.Count > 0 ? auction.Bids.MaxBy(b => b.Amount) : null;
                if (winningBid is not null)
                {
                    auction.WinnerId = winningBid.BidderId;

                    await notificationRepository.AddAsync(new Domain.Entities.Notification
                    {
                        UserId = winningBid.BidderId,
                        AuctionId = auction.Id,
                        Message = $"Congratulations! You won the auction \"{auction.Title}\" with a bid of {winningBid.Amount:C}."
                    }, cancellationToken);

                    pendingNotifications.Add((winningBid.BidderId, auction.Id, winningBid.Amount, IsWinner: true));
                }

                await notificationRepository.AddAsync(new Domain.Entities.Notification
                {
                    UserId = auction.SellerId,
                    AuctionId = auction.Id,
                    Message = winningBid is not null
                        ? $"Your auction \"{auction.Title}\" has closed. Winning bid: {winningBid.Amount:C}."
                        : $"Your auction \"{auction.Title}\" has closed with no bids."
                }, cancellationToken);

                pendingNotifications.Add((auction.SellerId, auction.Id, null, IsWinner: false));

                _logger.LogInformation("Auto-closed auction {AuctionId} ('{Title}').", auction.Id, auction.Title);
            }

            if (expiredAuctions.Count > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);

                foreach (var (userId, auctionId, amount, isWinner) in pendingNotifications)
                {
                    if (isWinner)
                        await notificationService.NotifyAuctionWonAsync(userId, auctionId, amount!.Value, cancellationToken);
                    else
                        await notificationService.NotifyAuctionClosedAsync(userId, auctionId, cancellationToken);
                }
            }
        }
    }
}
