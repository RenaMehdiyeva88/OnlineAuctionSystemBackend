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
    //
    // IMPORTANT: every DTO here is a C# record with a required-parameter
    // constructor (no parameterless ctor, no property setters). AutoMapper
    // needs .ForCtorParam(...) — NOT .ForMember(...) — to map into a
    // constructor parameter. Using .ForMember() on a record silently tells
    // AutoMapper to fall back to "new T() + property setters", which
    // crashes at runtime with "needs to have a constructor with 0 args or
    // only optional args" the moment it actually tries to build one,
    // because records have no such constructor.
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
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