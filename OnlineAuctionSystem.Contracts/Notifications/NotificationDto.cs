namespace OnlineAuctionSystem.Contracts.Notifications
{
    public record NotificationDto(
    Guid Id,
    string Message,
    bool IsRead,
    DateTime CreatedAt,
    Guid? AuctionId
);
}
