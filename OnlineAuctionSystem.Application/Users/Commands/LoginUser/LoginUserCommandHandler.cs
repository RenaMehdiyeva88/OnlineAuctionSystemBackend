using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Application.Users.Commands.LoginUser
{

    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTime _dateTime;

        // Refresh tokens are kept alive for 7 days by default.
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IDateTime dateTime)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
                ?? throw new UnauthorizedException("Invalid email or password.");

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // NOTE: requires User.RefreshToken / User.RefreshTokenExpiryTime on the Domain entity.
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _dateTime.UtcNow.Add(RefreshTokenLifetime);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(user.Id, user.Username, user.Role.ToString(), accessToken, refreshToken);
        }
    }
}
