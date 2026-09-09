using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Notifications.Commands.MarkNotificationAsRead;
using OnlineAuctionSystem.Application.Notifications.Queries.GetNotifications;
using OnlineAuctionSystem.Contracts.Common;
using OnlineAuctionSystem.Contracts.Notifications;
using System.Security.Claims;

namespace OnlineAuctionSystem.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly ISender _mediator;

        public NotificationsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // F3/F5 — paginated outbid and auction-won/closed notifications for the current user.
        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationDto>>> GetMyNotifications(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GetNotificationsQuery(userId, pageNumber, pageSize), cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _mediator.Send(new MarkNotificationAsReadCommand(id, userId), cancellationToken);
            return NoContent();
        }
    }
}
