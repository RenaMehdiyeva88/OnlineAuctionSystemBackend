using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Application.Users.Queries.GetSellerDashboard;
using System.Security.Claims;

namespace OnlineAuctionSystem.Presentation.Controllers
{

    // F6 — seller dashboard: manage active and completed auctions.
    [ApiController]
    [Authorize(Roles = "Seller")]
    [Route("api/seller/dashboard")]
    public class SellerDashboardController : ControllerBase
    {
        private readonly ISender _mediator;

        public SellerDashboardController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<SellerDashboardDto>> GetDashboard(CancellationToken cancellationToken)
        {
            var sellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GetSellerDashboardQuery(sellerId), cancellationToken);
            return Ok(result);
        }
    }
}
