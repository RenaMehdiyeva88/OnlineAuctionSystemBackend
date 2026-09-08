namespace OnlineAuctionSystem.Contracts.Users
{
    public record UserDto(
     Guid Id,
     string Username,
     string Email,
     string Role
 );
}
