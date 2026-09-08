using OnlineAuctionSystem.Application.Common.Interfaces.Services;

namespace OnlineAuctionSystem.Infrastructure.Identity
{
    // Requires NuGet package: BCrypt.Net-Next
    // Requires NuGet package: BCrypt.Net-Next
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
