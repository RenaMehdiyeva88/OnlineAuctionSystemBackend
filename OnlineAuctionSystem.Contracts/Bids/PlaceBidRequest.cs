namespace OnlineAuctionSystem.Contracts.Bids
{
    public record PlaceBidRequest(
     Guid AuctionId,
     decimal Amount
 );
}
