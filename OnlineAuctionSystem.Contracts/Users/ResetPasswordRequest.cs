namespace OnlineAuctionSystem.Contracts.Users
{
    public sealed record ResetPasswordRequest(
        string Email,
        string Token,
        string NewPassword);
}