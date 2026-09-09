using MediatR;
using OnlineAuctionSystem.Contracts.Bids;

namespace OnlineAuctionSystem.Application.Bids.Commands.PlaceBid
{
    // F3: Real-time bid placement with automatic outbid notifications
    public record PlaceBidCommand(
        Guid AuctionId,
        Guid BidderId,
        decimal Amount
    ) : IRequest<BidDto>;
}
