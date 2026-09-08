namespace OnlineAuctionSystem.Application.Common.Exceptions
{
    // F4: thrown when trying to bid on an auction that has already closed or expired.
    public class AuctionClosedException : Exception
    {
        public AuctionClosedException(Guid auctionId)
            : base($"Auction ({auctionId}) is already closed and no longer accepts bids.") { }
    }
}
