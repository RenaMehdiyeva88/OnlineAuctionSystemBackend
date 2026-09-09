using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Contracts.Common;
using OnlineAuctionSystem.Contracts.Notifications;

namespace OnlineAuctionSystem.Application.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public GetNotificationsQueryHandler(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var (notifications, totalCount) = await _notificationRepository.GetByUserIdAsync(
                request.UserId, request.PageNumber, request.PageSize, cancellationToken);

            return new PagedResult<NotificationDto>(
                _mapper.Map<List<NotificationDto>>(notifications), request.PageNumber, request.PageSize, totalCount);
        }
    }
}
