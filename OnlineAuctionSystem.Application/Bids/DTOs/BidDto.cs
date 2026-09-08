namespace OnlineAuctionSystem.Application.Bids.DTOs
{
    public class BidDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime BidTime { get; set; }
        public Guid BidderId { get; set; }
        public string BidderName { get; set; } = string.Empty;
        public Guid AuctionId { get; set; }
    }
}
