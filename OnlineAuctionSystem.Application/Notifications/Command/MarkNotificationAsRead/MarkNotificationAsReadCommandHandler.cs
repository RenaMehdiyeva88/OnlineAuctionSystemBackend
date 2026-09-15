using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Notifications.Commands.MarkNotificationAsRead
{
    // This handler was missing entirely — PATCH /api/notifications/{id}/read called
    // Mediator.Send(new MarkNotificationAsReadCommand(...)) with no handler registered
    // for it, so MediatR threw InvalidOperationException and every call to this
    // endpoint returned 500.
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Unit>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkNotificationAsReadCommandHandler(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken)
                ?? throw new NotFoundException(nameof(Notification), request.NotificationId);

            // IDOR guard — same pattern as CloseAuctionCommand: prove ownership
            // before mutating, don't trust the URL alone.
            if (notification.UserId != request.CurrentUserId)
                throw new ForbiddenException("You can only update your own notifications.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                _notificationRepository.Update(notification);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}