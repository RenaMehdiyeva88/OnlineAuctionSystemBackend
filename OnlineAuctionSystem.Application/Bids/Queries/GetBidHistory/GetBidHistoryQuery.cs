using MediatR;
using OnlineAuctionSystem.Application.Bids.DTOs;

namespace OnlineAuctionSystem.Application.Bids.Queries.GetBidHistory;

// F7: Bid history per auction item visible to all users
public record GetBidHistoryQuery(Guid AuctionId) : IRequest<List<BidDto>>;