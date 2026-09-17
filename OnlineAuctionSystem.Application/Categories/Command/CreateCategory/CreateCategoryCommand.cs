using MediatR;
using OnlineAuctionSystem.Contracts.Categories;

namespace OnlineAuctionSystem.Application.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand(string Name) : IRequest<CategoryDto>;
}
