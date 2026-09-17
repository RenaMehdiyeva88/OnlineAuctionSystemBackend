using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Application.Auctions.Commands.UpdateAuction
{
    public class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand, AuctionDto>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBidRepository _bidRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateAuctionCommandHandler(
            IAuctionRepository auctionRepository,
            IBidRepository bidRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _bidRepository = bidRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AuctionDto> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
        {
            var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken)
                ?? throw new NotFoundException(nameof(Auction), request.AuctionId);

            if (auction.SellerId != request.CurrentUserId)
                throw new ForbiddenException("You can only edit your own auctions.");

            if (auction.Status != AuctionStatus.Active)
                throw new ForbiddenException("Closed or cancelled auctions cannot be edited.");

            // Editing price/end-time/etc. after someone has already bid under
            // the OLD terms would be unfair to that bidder — once there's a
            // bid, the listing is locked.
            var existingBid = await _bidRepository.GetHighestBidAsync(auction.Id, cancellationToken);
            if (existingBid is not null)
                throw new ForbiddenException("This auction already has bids and can no longer be edited.");

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);

            auction.Title = request.Title;
            auction.Description = request.Description;
            auction.ImageUrl = request.ImageUrl;
            auction.StartingPrice = request.StartingPrice;
            auction.EndTime = request.EndTime;
            auction.CategoryId = category.Id;
            auction.MinimumIncrement = request.MinimumIncrement;

            _auctionRepository.Update(auction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            auction.Category = category;
            return _mapper.Map<AuctionDto>(auction);
        }
    }
}
