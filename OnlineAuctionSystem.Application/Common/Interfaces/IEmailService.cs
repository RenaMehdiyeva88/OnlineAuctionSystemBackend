namespace OnlineAuctionSystem.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(
            string email,
            string username,
            string resetLink,
            CancellationToken cancellationToken = default);
    }
}