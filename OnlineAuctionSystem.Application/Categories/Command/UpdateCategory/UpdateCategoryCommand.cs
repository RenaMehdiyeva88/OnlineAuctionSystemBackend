using MediatR;

namespace OnlineAuctionSystem.Application.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(Guid Id, string Name) : IRequest<Unit>;
}
