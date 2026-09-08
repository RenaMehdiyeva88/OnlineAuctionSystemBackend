using MediatR;
using OnlineAuctionSystem.Application.Auctions.DTOs;

namespace OnlineAuctionSystem.Application.Users.Queries.GetSellerDashboard
{
    // F6: Seller dashboard - manage active and completed auctions
    public record GetSellerDashboardQuery(Guid SellerId) : IRequest<SellerDashboardDto>;

    public record SellerDashboardDto(
        List<SellerAuctionDto> ActiveAuctions,
        List<SellerAuctionDto> CompletedAuctions
    );
}
