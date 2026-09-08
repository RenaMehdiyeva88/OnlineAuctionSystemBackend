namespace OnlineAuctionSystem.Application.Common.Exceptions
{
    // Maps to HTTP 401 (e.g. invalid login credentials, expired/invalid token).
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}
