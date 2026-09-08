namespace OnlineAuctionSystem.Contracts.Bids
{
    public record BidDto(
     Guid Id,
     decimal Amount,
     DateTime CreatedAt,
     string BidderName
 );
}
