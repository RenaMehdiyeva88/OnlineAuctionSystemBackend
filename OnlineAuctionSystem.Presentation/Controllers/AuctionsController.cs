using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Auctions.Commands.CloseAuction;
using OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction;
using OnlineAuctionSystem.Application.Auctions.DTOs;
using OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionById;
using OnlineAuctionSystem.Application.Auctions.Queries.GetAuctions;
using OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionsByCategory;
using OnlineAuctionSystem.Contracts.Auctions;
using System.Security.Claims;

namespace OnlineAuctionSystem.Presentation.Controllers
{

    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly ISender _mediator;

        public AuctionsController(ISender mediator)
        {
            _mediator = mediator;
        }

        // F8 — keyword + category + price range search. Anonymous: buyers browse without logging in.
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Application.Auctions.DTOs.AuctionListItemDto>>> Search(
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
        public async Task<ActionResult<Application.Auctions.DTOs.AuctionDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAuctionByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        // F8 — category-based browsing.
        [HttpGet("category/{categoryId:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Application.Auctions.DTOs.AuctionListItemDto>>> GetByCategory(Guid categoryId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAuctionsByCategoryQuery(
                null,
                categoryId,
                null,
                null,
                1,
                20), cancellationToken);
            return Ok(result);
        }

        // F2 — listing creation: title, description, starting price, end time. Sellers only.
        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<ActionResult<Application.Auctions.DTOs.AuctionDto>> Create(CreateAuctionRequest request, CancellationToken cancellationToken)
        {
            var sellerId = GetCurrentUserId();
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
            await _mediator.Send(new CloseAuctionCommand(id), cancellationToken);
            return NoContent();
        }

        private Guid GetCurrentUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
