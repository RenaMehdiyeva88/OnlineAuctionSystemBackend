using MediatR;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Application.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserDto>;
