using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Guid? ValidateAccessTokenAndGetUserId(string accessToken);
    }
}
