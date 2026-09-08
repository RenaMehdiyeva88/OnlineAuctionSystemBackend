using MediatR;
using OnlineAuctionSystem.Application.Users.DTOs;

namespace OnlineAuctionSystem.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponse>;