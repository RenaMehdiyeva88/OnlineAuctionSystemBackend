using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Bids.DTOs;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;

namespace OnlineAuctionSystem.Application.Bids.Queries.GetBidHistory
{
    public class GetBidHistoryQueryHandler : IRequestHandler<GetBidHistoryQuery, List<BidDto>>
    {
        private readonly IBidRepository _bidRepository;
        private readonly IMapper _mapper;

        public GetBidHistoryQueryHandler(IBidRepository bidRepository, IMapper mapper)
        {
            _bidRepository = bidRepository;
            _mapper = mapper;
        }

        public async Task<List<BidDto>> Handle(GetBidHistoryQuery request, CancellationToken cancellationToken)
        {
            var bids = await _bidRepository.GetByAuctionIdAsync(request.AuctionId, cancellationToken);
            return _mapper.Map<List<BidDto>>(bids);
        }
    }
}
