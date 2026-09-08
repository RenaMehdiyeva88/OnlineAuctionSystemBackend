using AutoMapper;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Application.Auctions.DTOs;
using OnlineAuctionSystem.Application.Bids.DTOs;
using OnlineAuctionSystem.Application.Categories.DTOs;
using OnlineAuctionSystem.Application.Notifications.DTOs;
using OnlineAuctionSystem.Application.Users.DTOs;

namespace OnlineAuctionSystem.Application.Common.Mappings
{

    // Single custom AutoMapper profile for the whole Application layer.
    // NOTE: handlers now map from in-memory lists returned by repositories
    // (List<Entity>), not from IQueryable<Entity> — so we use Mapper.Map(...)
    // rather than ProjectTo<T>(...), since repositories hide EF Core entirely.
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()));

            CreateMap<Category, CategoryDto>();

            CreateMap<Auction, AuctionDto>()
                .ForMember(d => d.CurrentHighestBid, opt => opt.MapFrom(s => s.CurrentHighestBid))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.SellerId, opt => opt.MapFrom(s => s.SellerId))
                .ForMember(d => d.SellerName, opt => opt.MapFrom(s => s.Seller.Username))
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
                .ForMember(d => d.WinnerName, opt => opt.MapFrom(s => s.Winner != null ? s.Winner.Username : null));

            CreateMap<Auction, AuctionListItemDto>()
                .ForMember(d => d.CurrentHighestBid, opt => opt.MapFrom(s => s.CurrentHighestBid))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.SellerId, opt => opt.MapFrom(s => s.SellerId))
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));

            // F6: seller dashboard projection
            CreateMap<Auction, SellerAuctionDto>()
                .ForMember(d => d.CurrentHighestBid, opt => opt.MapFrom(s => s.CurrentHighestBid))
                .ForMember(d => d.TotalBids, opt => opt.MapFrom(s => s.Bids.Count))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.WinnerId, opt => opt.MapFrom(s => s.WinnerId))
                .ForMember(d => d.WinnerName, opt => opt.MapFrom(s => s.Winner != null ? s.Winner.Username : null));

            CreateMap<Bid, BidDto>()
                .ForMember(d => d.BidderName, opt => opt.MapFrom(s => s.Bidder.Username));

            CreateMap<Notification, NotificationDto>();
        }
    }
}
