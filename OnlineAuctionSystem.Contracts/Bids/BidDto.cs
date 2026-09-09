namespace OnlineAuctionSystem.Contracts.Bids
{
    public record BidDto(
        Guid Id,
        decimal Amount,
        DateTime CreatedAt,
        Guid AuctionId,
        Guid BidderId,
        string BidderName
    );
}
