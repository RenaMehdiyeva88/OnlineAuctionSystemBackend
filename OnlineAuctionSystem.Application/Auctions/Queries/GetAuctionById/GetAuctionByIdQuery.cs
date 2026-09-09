using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionById;

public record GetAuctionByIdQuery(Guid AuctionId) : IRequest<AuctionDto>;