namespace OnlineAuctionSystem.Contracts.Auctions
{
    // Only allowed while an auction has zero bids (enforced in the handler) —
    // changing price/end-time after bidding has started would be unfair to
    // whoever already bid under the old terms.
    public record UpdateAuctionRequest(
        string Title,
        string Description,
        string? ImageUrl,
        decimal StartingPrice,
        DateTime EndTime,
        Guid CategoryId,
        decimal MinimumIncrement
    );
}
