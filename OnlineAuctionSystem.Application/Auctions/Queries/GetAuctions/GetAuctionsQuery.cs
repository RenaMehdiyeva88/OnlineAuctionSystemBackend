using MediatR;
using OnlineAuctionSystem.Application.Auctions.DTOs;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctions;

// General "browse all active auctions" listing, e.g. for a homepage feed.
public record GetAuctionsQuery(int Page = 1, int PageSize = 20) : IRequest<List<AuctionListItemDto>>;