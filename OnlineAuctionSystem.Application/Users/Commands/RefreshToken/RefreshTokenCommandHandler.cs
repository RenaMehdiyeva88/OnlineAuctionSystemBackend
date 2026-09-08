using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Users.DTOs;

namespace OnlineAuctionSystem.Application.Users.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandHandler(IUserRepository userRepository, ITokenService tokenService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var userId = _tokenService.ValidateAccessTokenAndGetUserId(request.AccessToken)
                ?? throw new UnauthorizedException("Invalid access token.");

            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken)
                ?? throw new UnauthorizedException("Invalid refresh token.");

            if (user.Id != userId || user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token is invalid or has expired.");

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(user.Id, user.Username, user.Role.ToString(), newAccessToken, newRefreshToken);
        }
    }
}
