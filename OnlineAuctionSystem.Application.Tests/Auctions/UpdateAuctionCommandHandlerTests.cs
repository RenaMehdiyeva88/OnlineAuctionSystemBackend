using FluentValidation;
using OnlineAuctionSystem.Application.Common;
using OnlineAuctionSystem.Application.Common.Interfaces;

namespace OnlineAuctionSystem.Application.Auctions.Commands.UpdateAuction
{
    public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
    {
        public UpdateAuctionCommandValidator(IDateTime dateTime)
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(150).MustNotContainHtml();
            RuleFor(x => x.Description).NotEmpty().MaximumLength(4000).MustNotContainHtml();
            RuleFor(x => x.StartingPrice).GreaterThan(0);
            RuleFor(x => x.MinimumIncrement).GreaterThan(0);
            RuleFor(x => x.EndTime).GreaterThan(dateTime.UtcNow)
                .WithMessage("End time must be in the future.");
            RuleFor(x => x.CategoryId).NotEmpty();
        }
    }
}