using MediatR;
using OnlineAuctionSystem.Contracts.Categories;

namespace OnlineAuctionSystem.Application.Categories.Queries.GetCategories;

// F8: needed to populate category filter/browse options
public record GetCategoriesQuery : IRequest<List<CategoryDto>>;