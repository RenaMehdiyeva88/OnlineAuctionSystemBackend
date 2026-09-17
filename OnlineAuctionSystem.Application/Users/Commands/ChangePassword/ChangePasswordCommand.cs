using MediatR;

namespace OnlineAuctionSystem.Application.Users.Commands.ChangePassword
{
    public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Unit>;
}
