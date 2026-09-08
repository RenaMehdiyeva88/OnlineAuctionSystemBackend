using FluentValidation;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction
{
    public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
    {
        public CreateAuctionCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
            RuleFor(x => x.StartingPrice).GreaterThan(0);
            RuleFor(x => x.EndTime).GreaterThan(DateTime.UtcNow)
                .WithMessage("End time must be in the future.");
            RuleFor(x => x.CategoryId).NotEmpty();
            RuleFor(x => x.SellerId).NotEmpty();
        }
    }
}
