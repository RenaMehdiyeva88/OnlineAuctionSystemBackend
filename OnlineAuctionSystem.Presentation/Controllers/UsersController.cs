using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Users.Queries.GetUserProfile;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Presentation.Controllers
{
    // GetUserProfileQuery/Handler existed in the Application layer but had no
    // controller action calling it — this endpoint was missing entirely, so
    // the profile page on the frontend had nothing real to call.
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ISender _mediator;

        public UsersController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserProfileQuery(id), cancellationToken);
            return Ok(result);
        }
    }
}
