using MediatR;

namespace OnlineAuctionSystem.Application.Users.Commands.ChangePassword
{
    public sealed record ChangePasswordCommand(
        string CurrentPassword,
        string NewPassword) : IRequest;
}