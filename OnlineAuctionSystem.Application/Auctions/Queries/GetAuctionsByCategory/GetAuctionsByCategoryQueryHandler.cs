using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Contracts.Common;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionsByCategory
{
    public class GetAuctionsByCategoryQueryHandler
    : IRequestHandler<GetAuctionsByCategoryQuery, PagedResult<AuctionListItemDto>>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IMapper _mapper;

        public GetAuctionsByCategoryQueryHandler(IAuctionRepository auctionRepository, IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<AuctionListItemDto>> Handle(GetAuctionsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var (auctions, totalCount) = await _auctionRepository.SearchAsync(
                request.Keyword, request.CategoryId, request.MinPrice, request.MaxPrice,
                request.Page, request.PageSize, cancellationToken);

            return new PagedResult<AuctionListItemDto>(
                _mapper.Map<List<AuctionListItemDto>>(auctions),
                request.Page,
                request.PageSize,
                totalCount);
        }
    }
}