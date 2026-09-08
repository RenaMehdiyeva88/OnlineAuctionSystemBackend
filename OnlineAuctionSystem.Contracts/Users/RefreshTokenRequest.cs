namespace OnlineAuctionSystem.Contracts.Users
{
    public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
}
