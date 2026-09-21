using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Common;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Contracts.Users;
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
        private readonly IDateTime _dateTime;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IDateTime dateTime,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
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
                Role = ParseAllowedRole(request.Role)
            };

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _dateTime.UtcNow.Add(AuthConstants.RefreshTokenLifetime);

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(user.Id, user.Username, user.Role.ToString(), accessToken, refreshToken);
        }

        private static UserRole ParseAllowedRole(string role)
        {
            if (role.Equals(nameof(UserRole.Buyer), StringComparison.OrdinalIgnoreCase))
                return UserRole.Buyer;
            if (role.Equals(nameof(UserRole.Seller), StringComparison.OrdinalIgnoreCase))
                return UserRole.Seller;

            throw new FluentValidation.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Role", "Role must be either 'Buyer' or 'Seller'.")
            });
        }
    }
}