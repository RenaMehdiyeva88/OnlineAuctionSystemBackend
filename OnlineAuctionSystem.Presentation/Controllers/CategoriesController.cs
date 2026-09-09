using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Contracts.Categories;
using OnlineAuctionSystem.Application.Categories.Queries.GetCategories;

namespace OnlineAuctionSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [AllowAnonymous]
    public class CategoriesController : ControllerBase
    {
        private readonly ISender _mediator;

        public CategoriesController(ISender mediator)
        {
            _mediator = mediator;
        }

        // F8 — category list for browsing/filter dropdowns.
        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
