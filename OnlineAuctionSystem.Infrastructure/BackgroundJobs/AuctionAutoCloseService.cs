using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineAuctionSystem.Application.Common;
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
            // Short-lived scope just to get the list of IDs — closed immediately
            // after, so its DbContext/ChangeTracker never lives across the loop.
            List<Guid> expiredAuctionIds;
            using (var listScope = _scopeFactory.CreateScope())
            {
                var listRepo = listScope.ServiceProvider.GetRequiredService<IAuctionRepository>();
                var expiredAuctions = await listRepo.GetExpiredActiveAuctionsAsync(cancellationToken);
                expiredAuctionIds = expiredAuctions.Select(a => a.Id).ToList();
            }

            foreach (var auctionId in expiredAuctionIds)
            {
                // A BRAND NEW scope (and therefore a brand new DbContext/
                // ChangeTracker) per auction. The old code shared ONE scope
                // for the entire batch: if auction #1 failed mid-save, its
                // half-modified entities stayed tracked as Modified/Added in
                // that same DbContext, and the next SaveChangesAsync (for
                // auction #2) tried to re-save auction #1's broken changes
                // too — one bad auction could silently block every other
                // auction in the batch from closing. A fresh scope per
                // auction means a failure is fully contained: its
                // ChangeTracker is disposed along with the scope and can
                // never leak into the next auction's save.
                using var itemScope = _scopeFactory.CreateScope();
                var auctionRepository = itemScope.ServiceProvider.GetRequiredService<IAuctionRepository>();
                var notificationRepository = itemScope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var notificationService = itemScope.ServiceProvider.GetRequiredService<INotificationService>();
                var unitOfWork = itemScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                try
                {
                    var auction = await auctionRepository.GetByIdAsync(auctionId, cancellationToken);
                    if (auction is null || auction.Status != AuctionStatus.Active)
                        continue; // already closed by a manual close or a previous tick

                    auction.Status = AuctionStatus.Closed;

                    var winningBid = auction.Bids.Count > 0 ? auction.Bids.MaxBy(b => b.Amount) : null;
                    if (winningBid is not null)
                    {
                        auction.WinnerId = winningBid.BidderId;

                        await notificationRepository.AddAsync(new Domain.Entities.Notification
                        {
                            UserId = winningBid.BidderId,
                            AuctionId = auction.Id,
                            Message = $"Congratulations! You won the auction \"{auction.Title}\" with a bid of {CurrencyFormatter.Format(winningBid.Amount)}."
                        }, cancellationToken);
                    }

                    await notificationRepository.AddAsync(new Domain.Entities.Notification
                    {
                        UserId = auction.SellerId,
                        AuctionId = auction.Id,
                        Message = winningBid is not null
                            ? $"Your auction \"{auction.Title}\" has closed. Winning bid: {CurrencyFormatter.Format(winningBid.Amount)}."
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
                    _logger.LogError(ex, "Failed to auto-close auction {AuctionId}. Will retry on next poll.", auctionId);
                }
            }
        }
    }
}
