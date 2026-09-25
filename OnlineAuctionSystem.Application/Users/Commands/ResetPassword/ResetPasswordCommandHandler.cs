using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace OnlineAuctionSystem.Application.Users.Commands.ResetPassword
{
    public sealed class ResetPasswordCommandHandler
        : IRequestHandler<ResetPasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(
            ResetPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(
                request.Email,
                cancellationToken);

            if (user == null)
                throw new UnauthorizedException(
                    "Invalid or expired password reset link.");

            if (string.IsNullOrWhiteSpace(
                    user.PasswordResetTokenHash))
            {
                throw new UnauthorizedException(
                    "Invalid or expired password reset link.");
            }

            if (!user.PasswordResetTokenExpiryTime.HasValue ||
                user.PasswordResetTokenExpiryTime.Value < DateTime.UtcNow)
            {
                throw new UnauthorizedException(
                    "Invalid or expired password reset link.");
            }

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(request.Token)));

            var valid = CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(user.PasswordResetTokenHash),
                Convert.FromHexString(tokenHash));

            if (!valid)
            {
                throw new UnauthorizedException(
                    "Invalid or expired password reset link.");
            }

            user.PasswordHash =
                _passwordHasher.Hash(request.NewPassword);

            // One-time token.
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiryTime = null;

            // Existing refresh token is invalidated.
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}