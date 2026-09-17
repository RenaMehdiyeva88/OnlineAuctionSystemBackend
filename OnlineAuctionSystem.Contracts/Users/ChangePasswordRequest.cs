namespace OnlineAuctionSystem.Contracts.Users
{
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}
