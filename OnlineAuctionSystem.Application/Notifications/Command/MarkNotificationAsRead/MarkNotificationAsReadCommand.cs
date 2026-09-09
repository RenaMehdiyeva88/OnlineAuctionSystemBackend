using MediatR;

namespace OnlineAuctionSystem.Application.Notifications.Commands.MarkNotificationAsRead;

// CurrentUserId comes from the JWT, never the URL/body — same IDOR lesson as
// CloseAuctionCommand: prove the notification belongs to the caller before
// mutating it.
public record MarkNotificationAsReadCommand(Guid NotificationId, Guid CurrentUserId) : IRequest<Unit>;
