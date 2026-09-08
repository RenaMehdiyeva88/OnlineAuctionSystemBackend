using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Notifications.DTOs;

namespace OnlineAuctionSystem.Application.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public GetNotificationsQueryHandler(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            return _mapper.Map<List<NotificationDto>>(notifications);
        }
    }
}
