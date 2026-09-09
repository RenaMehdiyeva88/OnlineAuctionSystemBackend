using MediatR;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Application.Users.Commands.RegisterUser
{
    // F1: User registration with seller and buyer roles
    public record RegisterUserCommand(
        string Username,
        string Email,
        string Password,
        string Role
    ) : IRequest<AuthResponse>;
}
