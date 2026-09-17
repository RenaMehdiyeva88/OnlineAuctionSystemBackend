using MediatR;

namespace OnlineAuctionSystem.Application.Categories.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;
}
