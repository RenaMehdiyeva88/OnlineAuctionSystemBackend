using FluentValidation;
using OnlineAuctionSystem.Application.Common.Interfaces;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction
{
    public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
    {
        public CreateAuctionCommandValidator(IDateTime dateTime)
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(150); // must match AuctionConfiguration.cs HasMaxLength(150)
            RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
            RuleFor(x => x.StartingPrice).GreaterThan(0);
            RuleFor(x => x.MinimumIncrement).GreaterThan(0);
            RuleFor(x => x.EndTime).GreaterThan(dateTime.UtcNow)
                .WithMessage("End time must be in the future.");
            RuleFor(x => x.CategoryId).NotEmpty();
            RuleFor(x => x.SellerId).NotEmpty();
        }
    }
}
