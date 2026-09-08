using MediatR;
using OnlineAuctionSystem.Application.Auctions.DTOs;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionById;

public record GetAuctionByIdQuery(Guid AuctionId) : IRequest<AuctionDto>;