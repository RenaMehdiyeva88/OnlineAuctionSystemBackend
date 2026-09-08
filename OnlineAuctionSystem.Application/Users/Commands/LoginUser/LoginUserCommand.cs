using MediatR;
using OnlineAuctionSystem.Application.Users.DTOs;

namespace OnlineAuctionSystem.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponse>;