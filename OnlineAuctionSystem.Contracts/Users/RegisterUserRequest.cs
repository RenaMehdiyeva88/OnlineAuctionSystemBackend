namespace OnlineAuctionSystem.Contracts.Users
{
    public record RegisterUserRequest(
    string Username,
    string Email,
    string Password,
    string Role // "Buyer" or "Seller"
);
}
