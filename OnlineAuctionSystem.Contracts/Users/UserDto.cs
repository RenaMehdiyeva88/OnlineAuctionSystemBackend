namespace OnlineAuctionSystem.Contracts.Users
{
    // NOTE: the deleted Application.Users.DTOs.UserDto had a "FullName" property
    // that never matched anything on the User entity (which only has Username) —
    // AutoMapper's convention-based mapping silently left it as "". Using
    // Username here (the real, populated field) fixes that latent bug.
    public record UserDto(
        Guid Id,
        string Username,
        string Email,
        string Role
    );
}
