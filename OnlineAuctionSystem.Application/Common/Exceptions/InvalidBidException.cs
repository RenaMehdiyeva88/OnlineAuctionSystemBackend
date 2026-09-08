namespace OnlineAuctionSystem.Application.Common.Exceptions
{
    // F3: thrown when a bid amount doesn't satisfy business rules (e.g. too low).
    public class InvalidBidException : Exception
    {
        public InvalidBidException(string message) : base(message) { }
    }
}
