using FluentValidation;

namespace OnlineAuctionSystem.Application.Bids.Commands.PlaceBid
{
    public class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
    {
        public PlaceBidCommandValidator()
        {
            RuleFor(x => x.AuctionId).NotEmpty();
            RuleFor(x => x.BidderId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
