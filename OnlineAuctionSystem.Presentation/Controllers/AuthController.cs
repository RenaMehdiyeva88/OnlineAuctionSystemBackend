using MediatR;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Users.Commands.LoginUser;
using OnlineAuctionSystem.Application.Users.Commands.RefreshToken;
using OnlineAuctionSystem.Application.Users.Commands.RegisterUser;
using OnlineAuctionSystem.Contracts.Users;

namespace OnlineAuctionSystem.Presentation.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _mediator;

        public AuthController(ISender mediator)
        {
            _mediator = mediator;
        }

        // F1 — registration with seller/buyer role.
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterUserCommand(
                request.Username,
                request.Email,
                request.Password,
                request.Role), cancellationToken);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LoginUserCommand(
                request.Email,
                request.Password), cancellationToken);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(
                request.AccessToken,
                request.RefreshToken), cancellationToken);
            return Ok(result);
        }
    }

}
