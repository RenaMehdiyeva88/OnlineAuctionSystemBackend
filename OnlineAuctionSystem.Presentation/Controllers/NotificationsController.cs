using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Notifications.Commands.MarkNotificationAsRead;
using OnlineAuctionSystem.Application.Notifications.Queries.GetNotifications;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Contracts.Common;
using OnlineAuctionSystem.Contracts.Notifications;

namespace OnlineAuctionSystem.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public NotificationsController(ISender mediator, ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _mediator = mediator;
        }

        // F3/F5 — paginated outbid and auction-won/closed notifications for the current user.
        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationDto>>> GetMyNotifications(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetNotificationsQuery(CurrentUserId, pageNumber, pageSize), cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MarkNotificationAsReadCommand(id, CurrentUserId), cancellationToken);
            return NoContent();
        }
    }
}