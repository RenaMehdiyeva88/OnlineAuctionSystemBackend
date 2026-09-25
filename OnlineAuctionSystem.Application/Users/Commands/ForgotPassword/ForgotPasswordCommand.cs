using MediatR;

namespace OnlineAuctionSystem.Application.Users.Commands.ForgotPassword
{
    public sealed record ForgotPasswordCommand(
        string Email) : IRequest;
}