using FluentValidation;
using OnlineAuctionSystem.Application.Users.Commands.ResetPassword;

namespace OnlineAuctionSystem.Application.Users.Validators
{
    public sealed class ResetPasswordCommandValidator
        : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Token)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .WithMessage(
                    "New password must contain at least 8 characters.");
        }
    }
}