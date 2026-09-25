using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Users.Commands.ChangePassword;
using OnlineAuctionSystem.Application.Users.Commands.ForgotPassword;
using OnlineAuctionSystem.Application.Users.Commands.LoginUser;
using OnlineAuctionSystem.Application.Users.Commands.RefreshToken;
using OnlineAuctionSystem.Application.Users.Commands.RegisterUser;
using OnlineAuctionSystem.Application.Users.Commands.ResetPassword;
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

        // Registration
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(
            RegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RegisterUserCommand(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.Role),
                cancellationToken);

            return Ok(result);
        }

        // Login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new LoginUserCommand(
                    request.Email,
                    request.Password),
                cancellationToken);

            return Ok(result);
        }

        // Refresh token
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(
            RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new RefreshTokenCommand(
                    request.AccessToken,
                    request.RefreshToken),
                cancellationToken);

            return Ok(result);
        }

        // Change password for authenticated user
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new ChangePasswordCommand(
                    request.CurrentPassword,
                    request.NewPassword),
                cancellationToken);

            return NoContent();
        }

        // Request password reset
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordRequest request,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new ForgotPasswordCommand(
                    request.Email),
                cancellationToken);

            return NoContent();
        }

        // Reset password using email + reset token
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new ResetPasswordCommand(
                    request.Email,
                    request.Token,
                    request.NewPassword),
                cancellationToken);

            return NoContent();
        }
    }
}