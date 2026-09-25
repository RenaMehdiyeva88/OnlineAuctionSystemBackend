namespace OnlineAuctionSystem.Contracts.Auth
{
    public sealed record ForgotPasswordRequest(
        string Email
    );
}