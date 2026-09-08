using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Auctions.DTOs;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Auctions.Queries.GetAuctionById
{
    public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, AuctionDto>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IMapper _mapper;

        public GetAuctionByIdQueryHandler(IAuctionRepository auctionRepository, IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _mapper = mapper;
        }

        public async Task<AuctionDto> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
        {
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken)
                ?? throw new NotFoundException(nameof(Auction), request.AuctionId);

            return _mapper.Map<AuctionDto>(auction);
        }
    }
}
