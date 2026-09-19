using AutoMapper;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Contracts.Bids;
using OnlineAuctionSystem.Contracts.Categories;
using OnlineAuctionSystem.Contracts.Notifications;
using OnlineAuctionSystem.Contracts.Users;
using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForCtorParam("Role", opt => opt.MapFrom(s => s.Role.ToString()));

            // Public projection — Email excluded — for viewing OTHER users'
            // profiles (GET /api/users/{id}). See PublicUserDto.
            CreateMap<User, PublicUserDto>()
                .ForCtorParam("Role", opt => opt.MapFrom(s => s.Role.ToString()));

            CreateMap<Category, CategoryDto>();

            CreateMap<Auction, AuctionDto>()
                .ForCtorParam("CurrentHighestBid", opt => opt.MapFrom(s => s.CurrentHighestBid))
                .ForCtorParam("Status", opt => opt.MapFrom(s => s.Status.ToString()))
                .ForCtorParam("SellerName", opt => opt.MapFrom(s => s.Seller.Username))
                .ForCtorParam("CategoryName", opt => opt.MapFrom(s => s.Category.Name))
                .ForCtorParam("WinnerName", opt => opt.MapFrom(s => s.Winner != null ? s.Winner.Username : null));

            CreateMap<Auction, AuctionListItemDto>()
                .ForCtorParam("CurrentHighestBid", opt => opt.MapFrom(s => s.CurrentHighestBid))
                .ForCtorParam("Status", opt => opt.MapFrom(s => s.Status.ToString()))
                .ForCtorParam("CategoryName", opt => opt.MapFrom(s => s.Category.Name));

            // F6: seller dashboard projection
            CreateMap<Auction, SellerAuctionDto>()
                .ForCtorParam("CurrentHighestBid", opt => opt.MapFrom(s => s.CurrentHighestBid))
                .ForCtorParam("TotalBids", opt => opt.MapFrom(s => s.Bids.Count))
                .ForCtorParam("Status", opt => opt.MapFrom(s => s.Status.ToString()))
                .ForCtorParam("WinnerName", opt => opt.MapFrom(s => s.Winner != null ? s.Winner.Username : null));

            CreateMap<Bid, BidDto>()
                .ForCtorParam("BidderName", opt => opt.MapFrom(s => s.Bidder.Username));

            CreateMap<Notification, NotificationDto>();
        }
    }
}