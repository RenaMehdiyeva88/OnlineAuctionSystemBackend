namespace OnlineAuctionSystem.Application.Auctions.DTOs
{
    public class AuctionListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal? CurrentHighestBid { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid SellerId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
