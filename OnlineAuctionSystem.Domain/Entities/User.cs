using OnlineAuctionSystem.Domain.Common;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public UserRole Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
