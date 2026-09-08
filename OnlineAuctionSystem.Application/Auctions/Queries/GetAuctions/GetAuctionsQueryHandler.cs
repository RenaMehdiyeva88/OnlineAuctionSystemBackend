using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Auctions.DTOs;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctions
{
    public class GetAuctionsQueryHandler : IRequestHandler<GetAuctionsQuery, List<AuctionListItemDto>>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IMapper _mapper;

        public GetAuctionsQueryHandler(IAuctionRepository auctionRepository, IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _mapper = mapper;
        }

        public async Task<List<AuctionListItemDto>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
        {
            var auctions = await _auctionRepository.GetAllActiveAsync(request.Page, request.PageSize, cancellationToken);
            return _mapper.Map<List<AuctionListItemDto>>(auctions);
        }
    }
}
