using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Application.Users.Queries.GetSellerDashboard
{

    public class GetSellerDashboardQueryHandler : IRequestHandler<GetSellerDashboardQuery, SellerDashboardDto>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IMapper _mapper;

        public GetSellerDashboardQueryHandler(IAuctionRepository auctionRepository, IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _mapper = mapper;
        }

        public async Task<SellerDashboardDto> Handle(GetSellerDashboardQuery request, CancellationToken cancellationToken)
        {
            // GetBySellerAsync no longer loads the Bids navigation at all
            // (see AuctionRepository) — bid counts/highest amounts come from
            // a separate SQL GROUP BY below instead of materializing every
            // bid row just to Count()/Max() them in memory.
            var auctions = await _auctionRepository.GetBySellerAsync(request.SellerId, cancellationToken);

            var bidStats = await _auctionRepository.GetBidStatsForAuctionsAsync(
                auctions.Select(a => a.Id), cancellationToken);

            var active = auctions.Where(a => a.Status == AuctionStatus.Active).ToList();
            var completed = auctions.Where(a => a.Status != AuctionStatus.Active).ToList();

            return new SellerDashboardDto(
                MapWithBidStats(active, bidStats),
                MapWithBidStats(completed, bidStats));
        }

        private List<SellerAuctionDto> MapWithBidStats(
            List<Domain.Entities.Auction> auctions,
            Dictionary<Guid, (int TotalBids, decimal? HighestBid)> bidStats)
        {
            var dtos = _mapper.Map<List<SellerAuctionDto>>(auctions);

            // AutoMapper's CurrentHighestBid/TotalBids mappings read the
            // Auction.Bids navigation, which is intentionally not loaded
            // here — patch both fields in from the aggregate query instead.
            for (var i = 0; i < auctions.Count; i++)
            {
                var stats = bidStats.TryGetValue(auctions[i].Id, out var s) ? s : (TotalBids: 0, HighestBid: (decimal?)null);
                dtos[i] = dtos[i] with
                {
                    TotalBids = stats.TotalBids,
                    CurrentHighestBid = stats.HighestBid ?? auctions[i].StartingPrice
                };
            }

            return dtos;
        }
    }
}