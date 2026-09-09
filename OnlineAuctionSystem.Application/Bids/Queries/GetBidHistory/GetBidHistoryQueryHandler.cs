using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Contracts.Bids;
using OnlineAuctionSystem.Contracts.Common;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;

namespace OnlineAuctionSystem.Application.Bids.Queries.GetBidHistory
{
    public class GetBidHistoryQueryHandler : IRequestHandler<GetBidHistoryQuery, PagedResult<BidDto>>
    {
        private readonly IBidRepository _bidRepository;
        private readonly IMapper _mapper;

        public GetBidHistoryQueryHandler(IBidRepository bidRepository, IMapper mapper)
        {
            _bidRepository = bidRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<BidDto>> Handle(GetBidHistoryQuery request, CancellationToken cancellationToken)
        {
            var (bids, totalCount) = await _bidRepository.GetByAuctionIdAsync(
                request.AuctionId, request.PageNumber, request.PageSize, cancellationToken);

            return new PagedResult<BidDto>(
                _mapper.Map<List<BidDto>>(bids), request.PageNumber, request.PageSize, totalCount);
        }
    }
}
