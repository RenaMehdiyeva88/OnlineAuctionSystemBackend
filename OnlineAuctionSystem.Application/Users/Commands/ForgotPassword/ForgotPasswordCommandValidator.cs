using FluentValidation;
using OnlineAuctionSystem.Application.Users.Commands.ForgotPassword;

namespace OnlineAuctionSystem.Application.Users.Validators
{
    public sealed class ForgotPasswordCommandValidator
        : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("A valid email is required.");
        }
    }
}