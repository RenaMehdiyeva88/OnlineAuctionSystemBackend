using MediatR;
using Microsoft.Extensions.Configuration;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace OnlineAuctionSystem.Application.Users.Commands.ForgotPassword
{
    public sealed class ForgotPasswordCommandHandler
        : IRequestHandler<ForgotPasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task Handle(
            ForgotPasswordCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(
                request.Email,
                cancellationToken);

            // Do not reveal whether the email exists.
            if (user == null)
                return;

            var tokenBytes = RandomNumberGenerator.GetBytes(32);

            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(token)));

            user.PasswordResetTokenHash = tokenHash;

            user.PasswordResetTokenExpiryTime =
                DateTime.UtcNow.AddMinutes(30);

            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var frontendUrl =
                _configuration["Frontend:BaseUrl"]
                ?? "http://localhost:5173";

            var resetLink =
                $"{frontendUrl}/reset-password" +
                $"?email={Uri.EscapeDataString(user.Email)}" +
                $"&token={Uri.EscapeDataString(token)}";

            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.Username,
                resetLink,
                cancellationToken);
        }
    }
}