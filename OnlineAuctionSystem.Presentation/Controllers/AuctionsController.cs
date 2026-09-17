using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Auctions.Commands.CloseAuction;
using OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction;
using OnlineAuctionSystem.Application.Auctions.Commands.UpdateAuction;
using OnlineAuctionSystem.Application.Auctions.Commands.CancelAuction;
using OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionById;
using OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionsByCategory;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Contracts.Common;

namespace OnlineAuctionSystem.Presentation.Controllers
{

    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public AuctionsController(ISender mediator, ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _mediator = mediator;
        }

        // F8 — keyword + category + price range search. Anonymous: buyers browse without logging in.
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<AuctionListItemDto>>> Search(
            [FromQuery] AuctionSearchRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAuctionsByCategoryQuery(
                request.Keyword,
                request.CategoryId,
                request.MinPrice,
                request.MaxPrice,
                request.Page,
                request.PageSize), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<AuctionDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAuctionByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        // F8 — category-based browsing. "page" here now matches Search's
        // parameter name (was "pageNumber") — same endpoint family, different
        // parameter names was a real API inconsistency the frontend had to
        // special-case around.
        [HttpGet("category/{categoryId:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<AuctionListItemDto>>> GetByCategory(
            Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAuctionsByCategoryQuery(
                null,
                categoryId,
                null,
                null,
                page,
                pageSize), cancellationToken);
            return Ok(result);
        }

        // F2 — listing creation: title, description, starting price, end time. Sellers only.
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<AuctionDto>> Create(CreateAuctionRequest request, CancellationToken cancellationToken)
        {
            var sellerId = CurrentUserId;
            var result = await _mediator.Send(new CreateAuctionCommand(
                request.Title,
                request.Description,
                request.ImageUrl,
                request.StartingPrice,
                request.EndTime,
                request.CategoryId,
                sellerId,
                request.MinimumIncrement), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // Edit — only while the auction has zero bids (enforced in the handler).
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<AuctionDto>> Update(Guid id, UpdateAuctionRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateAuctionCommand(
                id,
                CurrentUserId,
                request.Title,
                request.Description,
                request.ImageUrl,
                request.StartingPrice,
                request.EndTime,
                request.CategoryId,
                request.MinimumIncrement), cancellationToken);
            return Ok(result);
        }

        // Cancel — only while the auction has zero bids (enforced in the handler).
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new CancelAuctionCommand(id, CurrentUserId), cancellationToken);
            return NoContent();
        }

        // F4/F5 — manual close (in addition to the background auto-close job). Seller-only, own auction.
        [HttpPost("{id:guid}/close")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new CloseAuctionCommand(id, CurrentUserId), cancellationToken);
            return NoContent();
        }

    }
}
