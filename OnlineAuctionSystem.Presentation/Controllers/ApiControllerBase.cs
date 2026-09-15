using Microsoft.AspNetCore.Mvc;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;

namespace OnlineAuctionSystem.Presentation.Controllers
{
    // Shared base for every [Authorize]-protected controller. Replaces the
    // duplicated `Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`
    // pattern that used to live in AuctionsController, BidsController, and
    // NotificationsController — that "!" (null-forgiving operator) meant a
    // missing/malformed claim crashed with an unhandled NullReferenceException
    // and returned 500 instead of a proper 401. It also finally puts the
    // already-registered-but-unused ICurrentUserService to work instead of
    // reading claims directly in three different places.
    public abstract class ApiControllerBase : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        protected ApiControllerBase(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        // Every action that reaches this point is already behind [Authorize],
        // so a null UserId here means a malformed/missing claim rather than
        // "not logged in" — still surfaced as a clean 401, never a 500.
        protected Guid CurrentUserId =>
            _currentUserService.UserId
                ?? throw new UnauthorizedException("No valid user identifier found on the current request.");
    }
}