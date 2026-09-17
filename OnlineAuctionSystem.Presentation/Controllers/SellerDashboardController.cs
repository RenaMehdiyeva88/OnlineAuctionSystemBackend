using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Application.Users.Queries.GetSellerDashboard;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;

namespace OnlineAuctionSystem.Presentation.Controllers
{

    // F6 — seller dashboard: manage active and completed auctions.
    [ApiController]
    [Authorize(Roles = "Seller")]
    [Route("api/seller/dashboard")]
    public class SellerDashboardController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public SellerDashboardController(ISender mediator, ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<SellerDashboardDto>> GetDashboard(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSellerDashboardQuery(CurrentUserId), cancellationToken);
            return Ok(result);
        }
    }
}
