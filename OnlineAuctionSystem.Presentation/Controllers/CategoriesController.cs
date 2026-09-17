using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Contracts.Categories;
using OnlineAuctionSystem.Application.Categories.Queries.GetCategories;
using OnlineAuctionSystem.Application.Categories.Commands.CreateCategory;
using OnlineAuctionSystem.Application.Categories.Commands.UpdateCategory;
using OnlineAuctionSystem.Application.Categories.Commands.DeleteCategory;

namespace OnlineAuctionSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ISender _mediator;

        public CategoriesController(ISender mediator)
        {
            _mediator = mediator;
        }

        // F8 — category list for browsing/filter dropdowns. Public.
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
            return Ok(result);
        }

        // Everything below is Admin-only category management.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateCategoryCommand(request.Name), cancellationToken);
            return CreatedAtAction(nameof(GetAll), result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(new UpdateCategoryCommand(id, request.Name), cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
