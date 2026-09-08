namespace OnlineAuctionSystem.Application.Notifications.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public Guid UserId { get; set; }
    }
}
