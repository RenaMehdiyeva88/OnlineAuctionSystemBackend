using MediatR;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponse>;