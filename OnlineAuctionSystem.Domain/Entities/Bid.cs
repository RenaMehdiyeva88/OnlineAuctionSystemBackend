using OnlineAuctionSystem.Domain.Common;

namespace OnlineAuctionSystem.Domain.Entities
{
    public class Bid : BaseEntity
    {
        public decimal Amount { get; set; }

        public Guid AuctionId { get; set; }
        public Auction Auction { get; set; } = default!;

        public Guid BidderId { get; set; }
        public User Bidder { get; set; } = default!;
    }
}
