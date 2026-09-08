using OnlineAuctionSystem.Domain.Common;

namespace OnlineAuctionSystem.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = default!;

        public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
    }
}
