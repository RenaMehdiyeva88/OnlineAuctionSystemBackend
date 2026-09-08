using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Notifications.DTOs;
using OnlineAuctionSystem.Application.Notifications.Queries.GetNotifications;
using OnlineAuctionSystem.Domain.Entities;
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

        // F3/F5 — outbid and auction-won/closed notifications for the current user.
        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetMyNotifications(CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GetNotificationsQuery(userId), cancellationToken);
            return Ok(result);
        }
    }
}
