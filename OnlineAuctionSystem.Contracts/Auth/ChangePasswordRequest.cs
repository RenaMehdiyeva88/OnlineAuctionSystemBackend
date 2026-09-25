namespace OnlineAuctionSystem.Contracts.Auth
{
    public sealed record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
}