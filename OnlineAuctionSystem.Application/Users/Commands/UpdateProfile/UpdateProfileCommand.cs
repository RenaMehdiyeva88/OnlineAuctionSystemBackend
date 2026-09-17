using MediatR;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Application.Users.Commands.UpdateProfile
{
    public record UpdateProfileCommand(Guid UserId, string Username, string Email) : IRequest<UserDto>;
}
