using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Auctions.DTOs;
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
            var auctions = await _auctionRepository.GetBySellerAsync(request.SellerId, cancellationToken);

            var active = auctions.Where(a => a.Status == AuctionStatus.Active).ToList();
            var completed = auctions.Where(a => a.Status != AuctionStatus.Active).ToList();

            return new SellerDashboardDto(
                _mapper.Map<List<SellerAuctionDto>>(active),
                _mapper.Map<List<SellerAuctionDto>>(completed));
        }
    }
}
