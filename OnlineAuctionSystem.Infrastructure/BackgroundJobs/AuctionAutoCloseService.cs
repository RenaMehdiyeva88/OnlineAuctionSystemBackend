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

            // Each auction is closed and saved INDIVIDUALLY, inside its own
            // try/catch. The old code mutated every expired auction in
            // memory and called SaveChangesAsync ONCE for the whole batch —
            // if even one auction hit a concurrency conflict or FK issue,
            // that single SaveChangesAsync threw and rolled back every other
            // auction in the batch too, silently leaving otherwise-fine
            // auctions still "Active" just because one had a problem. Now a
            // failure on one auction is logged and skipped; the rest still
            // close normally on this pass (and a still-broken one is simply
            // retried on the next 30s tick, since it stays in
            // GetExpiredActiveAuctionsAsync's results until it succeeds).
            foreach (var auction in expiredAuctions)
            {
                try
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
                    }

                    await notificationRepository.AddAsync(new Domain.Entities.Notification
                    {
                        UserId = auction.SellerId,
                        AuctionId = auction.Id,
                        Message = winningBid is not null
                            ? $"Your auction \"{auction.Title}\" has closed. Winning bid: {winningBid.Amount:C}."
                            : $"Your auction \"{auction.Title}\" has closed with no bids."
                    }, cancellationToken);

                    // Save THIS auction's changes before notifying — same
                    // save-before-notify ordering as CloseAuctionCommandHandler,
                    // just scoped to a single auction instead of the whole batch.
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    if (winningBid is not null)
                    {
                        await notificationService.NotifyAuctionWonAsync(
                            winningBid.BidderId, auction.Id, winningBid.Amount, cancellationToken);
                    }
                    await notificationService.NotifyAuctionClosedAsync(auction.SellerId, auction.Id, cancellationToken);

                    _logger.LogInformation("Auto-closed auction {AuctionId} ('{Title}').", auction.Id, auction.Title);
                }
                catch (Exception ex)
                {
                    // Don't let one bad auction block the rest of the batch —
                    // log it and move on; it stays "Active" and will be
                    // picked up again on the next poll.
                    _logger.LogError(ex, "Failed to auto-close auction {AuctionId}. Will retry on next poll.", auction.Id);
                }
            }
        }
    }
}