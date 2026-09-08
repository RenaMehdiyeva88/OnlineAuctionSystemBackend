using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Bids.Commands.PlaceBid;
using OnlineAuctionSystem.Application.Bids.DTOs;
using OnlineAuctionSystem.Application.Bids.Queries.GetBidHistory;
using OnlineAuctionSystem.Contracts.Bids;
using System.Security.Claims;

namespace OnlineAuctionSystem.Presentation.Controllers
{

    [ApiController]
    [Route("api")]
    public class BidsController : ControllerBase
    {
        private readonly ISender _mediator;

        public BidsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // F3 — real-time bid placement; outbid notifications are pushed from the handler via INotificationService.
        [HttpPost("bids")]
        [Authorize(Roles = "Buyer")]
        public async Task<ActionResult<Application.Bids.DTOs.BidDto>> PlaceBid(PlaceBidRequest request, CancellationToken cancellationToken)
        {
            var bidderId = GetCurrentUserId();
            var result = await _mediator.Send(new PlaceBidCommand(
                request.AuctionId,
                bidderId,
                request.Amount), cancellationToken);
            return Ok(result);
        }

        // F7 — full bid history, visible to all users (no auth required).
        [HttpGet("auctions/{auctionId:guid}/bids")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Application.Bids.DTOs.BidDto>>> GetHistory(Guid auctionId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetBidHistoryQuery(auctionId), cancellationToken);
            return Ok(result);
        }

        private Guid GetCurrentUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}