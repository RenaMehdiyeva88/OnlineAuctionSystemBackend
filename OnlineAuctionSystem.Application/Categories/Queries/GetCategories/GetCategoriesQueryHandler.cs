using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Categories.DTOs;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;

namespace OnlineAuctionSystem.Application.Categories.Queries.GetCategories
{
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<List<CategoryDto>>(categories);
        }
    }
}
