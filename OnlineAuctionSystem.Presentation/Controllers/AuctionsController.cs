using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Auctions.Commands.CloseAuction;
using OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction;
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

        // F8 — category-based browsing.
        [HttpGet("category/{categoryId:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<AuctionListItemDto>>> GetByCategory(
            Guid categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetAuctionsByCategoryQuery(
                null,
                categoryId,
                null,
                null,
                pageNumber,
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
                sellerId), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // F4/F5 — manual close (in addition to the background auto-close job). Seller-only, own auction.
        [HttpPost("{id:guid}/close")]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
        {
            // Ownership is enforced in CloseAuctionCommandHandler by comparing
            // this to auction.SellerId — never trust an ID from the client here.
            var currentUserId = CurrentUserId;
            await _mediator.Send(new CloseAuctionCommand(id, currentUserId), cancellationToken);
            return NoContent();
        }

    }
}