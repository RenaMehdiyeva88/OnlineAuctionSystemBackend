using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Application.Users.DTOs;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Application.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        // Refresh tokens are kept alive for 7 days by default.
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
                throw new ConflictException("A user with this email already exists.");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = Enum.Parse<UserRole>(request.Role, true)
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate tokens for the newly registered user
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Store refresh token on the user entity
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.Add(RefreshTokenLifetime);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(user.Id, user.Username, user.Role.ToString(), accessToken, refreshToken);
        }
    }
}
