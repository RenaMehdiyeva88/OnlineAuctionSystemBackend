using MediatR;
using OnlineAuctionSystem.Contracts.Bids;
using OnlineAuctionSystem.Contracts.Common;

namespace OnlineAuctionSystem.Application.Bids.Queries.GetBidHistory;

// F7: Bid history per auction item visible to all users
public record GetBidHistoryQuery(Guid AuctionId, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<BidDto>>;