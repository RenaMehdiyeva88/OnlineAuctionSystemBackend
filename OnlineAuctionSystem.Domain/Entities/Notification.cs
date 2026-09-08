using OnlineAuctionSystem.Domain.Common;

namespace OnlineAuctionSystem.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string Message { get; set; } = default!;
        public bool IsRead { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public Guid? AuctionId { get; set; }
        public Auction? Auction { get; set; }
    }
}
