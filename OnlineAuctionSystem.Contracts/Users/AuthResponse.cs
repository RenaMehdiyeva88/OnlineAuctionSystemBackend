namespace OnlineAuctionSystem.Contracts.Users
{
    public record AuthResponse(
    Guid UserId,
    string Username,
    string Role,
    string AccessToken,
    string RefreshToken
);
}
