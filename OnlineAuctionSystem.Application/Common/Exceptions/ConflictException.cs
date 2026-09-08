namespace OnlineAuctionSystem.Application.Common.Exceptions
{
    // Maps to HTTP 409 (e.g. registering with an email that already exists).
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
