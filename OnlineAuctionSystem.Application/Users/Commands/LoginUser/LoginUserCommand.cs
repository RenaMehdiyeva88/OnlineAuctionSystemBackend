using MediatR;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponse>;