using MediatR;
using OnlineAuctionSystem.Contracts.Common;
using OnlineAuctionSystem.Contracts.Notifications;

namespace OnlineAuctionSystem.Application.Notifications.Queries.GetNotifications;

// F3/F5: lets a user see their outbid / auction-won / auction-closed notification history
public record GetNotificationsQuery(Guid UserId, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<NotificationDto>>;
