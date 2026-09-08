using FluentValidation;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Application.Users.Commands.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.Role)
                .Must(r => Enum.TryParse<UserRole>(r, true, out _))
                .WithMessage("Role must be either 'Buyer' or 'Seller'.");
        }
    }
}
