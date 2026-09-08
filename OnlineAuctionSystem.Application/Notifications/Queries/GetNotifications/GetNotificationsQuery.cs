using MediatR;
using OnlineAuctionSystem.Application.Notifications.DTOs;

namespace OnlineAuctionSystem.Application.Notifications.Queries.GetNotifications;

// F3/F5: lets a user see their outbid / auction-won / auction-closed notification history
public record GetNotificationsQuery(Guid UserId) : IRequest<List<NotificationDto>>;
