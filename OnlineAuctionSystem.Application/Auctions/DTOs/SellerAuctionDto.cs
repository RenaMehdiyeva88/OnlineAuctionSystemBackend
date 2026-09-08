namespace OnlineAuctionSystem.Application.Auctions.DTOs
{
    public class SellerAuctionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal StartingPrice { get; set; }
        public decimal? CurrentHighestBid { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalBids { get; set; }
        public Guid? WinnerId { get; set; }
        public string? WinnerName { get; set; }
    }
}
