using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Application.Users.Queries.GetUserProfile;
using OnlineAuctionSystem.Application.Users.Commands.UpdateProfile;
using OnlineAuctionSystem.Application.Users.Commands.ChangePassword;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersController : ApiControllerBase
    {
        private readonly ISender _mediator;

        public UsersController(ISender mediator, ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _mediator = mediator;
        }

        // Must be registered before {id:guid}, or ASP.NET Core tries (and
        // fails) to parse "me" as a Guid.
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserProfileQuery(CurrentUserId), cancellationToken);
            return Ok(result);
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new UpdateProfileCommand(CurrentUserId, request.Username, request.Email), cancellationToken);
            return Ok(result);
        }

        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ChangePasswordCommand(CurrentUserId, request.CurrentPassword, request.NewPassword), cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserProfileQuery(id), cancellationToken);
            return Ok(result);
        }
    }
}
