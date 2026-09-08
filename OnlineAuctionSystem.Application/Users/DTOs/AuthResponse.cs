namespace OnlineAuctionSystem.Application.Users.DTOs
{
    public record AuthResponse(
        Guid UserId,
        string Username,
        string Role,
        string AccessToken,
        string RefreshToken
    );
}
