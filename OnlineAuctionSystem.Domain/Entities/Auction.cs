using OnlineAuctionSystem.Domain.Common;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Domain.Entities
{
    public class Auction : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public decimal StartingPrice { get; set; }
        public DateTime EndTime { get; set; }
        public AuctionStatus Status { get; set; } = AuctionStatus.Active;

        public Guid SellerId { get; set; }
        public User Seller { get; set; } = default!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public Guid? WinnerId { get; set; }
        public User? Winner { get; set; }

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();

        public decimal CurrentHighestBid => Bids.Count > 0 ? Bids.Max(b => b.Amount) : StartingPrice;

        public bool IsExpired => DateTime.UtcNow >= EndTime;
    }

}
