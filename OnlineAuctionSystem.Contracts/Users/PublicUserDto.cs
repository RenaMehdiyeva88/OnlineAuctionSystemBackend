namespace OnlineAuctionSystem.Contracts.Users
{
    public record PublicUserDto(
        Guid Id,
        string Username,
        string Role
    );
}