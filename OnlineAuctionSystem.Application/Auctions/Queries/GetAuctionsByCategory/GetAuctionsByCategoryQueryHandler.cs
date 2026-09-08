using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Auctions.DTOs;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionsByCategory
{
    public class GetAuctionsByCategoryQueryHandler
    : IRequestHandler<GetAuctionsByCategoryQuery, List<AuctionListItemDto>>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IMapper _mapper;

        public GetAuctionsByCategoryQueryHandler(IAuctionRepository auctionRepository, IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _mapper = mapper;
        }

        public async Task<List<AuctionListItemDto>> Handle(GetAuctionsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var auctions = await _auctionRepository.SearchAsync(
                request.Keyword, request.CategoryId, request.MinPrice, request.MaxPrice,
                request.Page, request.PageSize, cancellationToken);

            return _mapper.Map<List<AuctionListItemDto>>(auctions);
        }
    }
}
