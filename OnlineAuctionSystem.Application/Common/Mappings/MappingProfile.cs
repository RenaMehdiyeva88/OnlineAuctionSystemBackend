using AutoMapper;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Contracts.Bids;
using OnlineAuctionSystem.Contracts.Categories;
using OnlineAuctionSystem.Contracts.Notifications;
using OnlineAuctionSystem.Contracts.Users;
using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Mappings
{

    // Single custom AutoMapper profile for the whole Application layer.
    // All response DTOs live in OnlineAuctionSystem.Contracts (single source of
    // truth — there used to be a duplicate, slightly-diverged set of classes
    // under Application/*/DTOs, which this profile no longer references).
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
                .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
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
                .ForMember(d => d.AuctionId, opt => opt.MapFrom(s => s.AuctionId))
                .ForMember(d => d.BidderId, opt => opt.MapFrom(s => s.BidderId))
                .ForMember(d => d.BidderName, opt => opt.MapFrom(s => s.Bidder.Username));

            CreateMap<Notification, NotificationDto>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId));
        }
    }
}
