using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using OnlineAuctionSystem.Application.Common.Interfaces;

namespace OnlineAuctionSystem.Infrastructure.Services
{
    public sealed class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(
            string email,
            string username,
            string resetLink,
            CancellationToken cancellationToken = default)
        {
            var host = _configuration["Email:Host"]
                ?? throw new InvalidOperationException(
                    "Email:Host is not configured.");

            var port = int.Parse(
                _configuration["Email:Port"] ?? "587");

            var usernameConfig =
                _configuration["Email:Username"]
                ?? throw new InvalidOperationException(
                    "Email:Username is not configured.");

            var password =
                _configuration["Email:Password"]
                ?? throw new InvalidOperationException(
                    "Email:Password is not configured.");

            var from =
                _configuration["Email:From"]
                ?? usernameConfig;

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    "Online Auction System",
                    from));

            message.To.Add(
                MailboxAddress.Parse(email));

            message.Subject =
                "Reset your Online Auction password";

            var safeUsername =
                System.Net.WebUtility.HtmlEncode(username);

            var safeResetLink =
                System.Net.WebUtility.HtmlEncode(resetLink);

            message.Body = new BodyBuilder
            {
                HtmlBody = $"""
                    <html>
                    <body>
                        <h2>Password Reset</h2>

                        <p>
                            Hello {safeUsername},
                        </p>

                        <p>
                            We received a request to reset your
                            Online Auction System password.
                        </p>

                        <p>
                            <a href="{safeResetLink}">
                                Reset Password
                            </a>
                        </p>

                        <p>
                            This link expires in 30 minutes.
                        </p>

                        <p>
                            If you did not request this,
                            you can safely ignore this email.
                        </p>
                    </body>
                    </html>
                    """
            }.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                host,
                port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            await smtp.AuthenticateAsync(
                usernameConfig,
                password,
                cancellationToken);

            await smtp.SendAsync(
                message,
                cancellationToken);

            await smtp.DisconnectAsync(
                true,
                cancellationToken);
        }
    }
}