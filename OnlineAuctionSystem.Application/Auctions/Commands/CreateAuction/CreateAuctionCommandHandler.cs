using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;

namespace OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction
{
    public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, AuctionDto>
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateAuctionCommandHandler(
            IAuctionRepository auctionRepository,
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _auctionRepository = auctionRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AuctionDto> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
        {
            var seller = await _userRepository.GetByIdAsync(request.SellerId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.SellerId);

            if (seller.Role != UserRole.Seller)
                throw new ForbiddenException("Only users with the Seller role can create auctions.");

            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);

            var auction = new Auction
            {
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                StartingPrice = request.StartingPrice,
                EndTime = request.EndTime,
                CategoryId = category.Id,
                SellerId = seller.Id,
                Status = AuctionStatus.Active
            };

            await _auctionRepository.AddAsync(auction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            auction.Seller = seller;
            auction.Category = category;

            return _mapper.Map<AuctionDto>(auction);
        }
    }
}
