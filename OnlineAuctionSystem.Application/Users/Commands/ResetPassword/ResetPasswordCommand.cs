using MediatR;

namespace OnlineAuctionSystem.Application.Users.Commands.ResetPassword
{
    public sealed record ResetPasswordCommand(
        string Email,
        string Token,
        string NewPassword
    ) : IRequest;
}