using MediatR;
using OnlineAuctionSystem.Application.Users.DTOs;

namespace OnlineAuctionSystem.Application.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserDto>;
