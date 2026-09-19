using MediatR;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Application.Users.Queries.GetPublicUserProfile;

public record GetPublicUserProfileQuery(Guid UserId) : IRequest<PublicUserDto>;