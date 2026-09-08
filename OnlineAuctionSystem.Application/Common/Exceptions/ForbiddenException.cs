namespace OnlineAuctionSystem.Application.Common.Exceptions
{
    // Maps to HTTP 403. The user is authenticated but not allowed to perform this action
    // (e.g. a non-seller creating an auction, a seller bidding on their own auction).
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
