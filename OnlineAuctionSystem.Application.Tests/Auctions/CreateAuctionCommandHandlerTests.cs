using AutoMapper;
using Moq;
using OnlineAuctionSystem.Application.Auctions.Commands.CreateAuction;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Contracts.Auctions;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using Xunit;

namespace OnlineAuctionSystem.Application.Tests.Auctions
{
    public class CreateAuctionCommandHandlerTests
    {
        private readonly Mock<IAuctionRepository> _auctionRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<ICategoryRepository> _categoryRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IMapper> _mapper = new();

        private readonly CreateAuctionCommandHandler _handler;

        public CreateAuctionCommandHandlerTests()
        {
            _handler = new CreateAuctionCommandHandler(
                _auctionRepository.Object,
                _userRepository.Object,
                _categoryRepository.Object,
                _unitOfWork.Object,
                _mapper.Object);
        }

        private static User Seller(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Username = "seller1",
            Email = "seller1@test.com",
            PasswordHash = "hash",
            Role = UserRole.Seller
        };

        private static Category ValidCategory() => new() { Id = Guid.NewGuid(), Name = "Electronics" };

        [Fact]
        public async Task Handle_SellerDoesNotExist_ThrowsNotFoundException()
        {
            var sellerId = Guid.NewGuid();
            _userRepository.Setup(r => r.GetByIdAsync(sellerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new CreateAuctionCommand(
                "Title", "Description", null, 100m, DateTime.UtcNow.AddDays(1), Guid.NewGuid(), sellerId, 1.00m);

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UserIsNotSellerRole_ThrowsForbiddenException()
        {
            var buyer = new User
            {
                Id = Guid.NewGuid(),
                Username = "buyer1",
                Email = "buyer1@test.com",
                PasswordHash = "hash",
                Role = UserRole.Buyer
            };
            _userRepository.Setup(r => r.GetByIdAsync(buyer.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buyer);

            var command = new CreateAuctionCommand(
                "Title", "Description", null, 100m, DateTime.UtcNow.AddDays(1), Guid.NewGuid(), buyer.Id, 1.00m);

            await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command, CancellationToken.None));

            _auctionRepository.Verify(r => r.AddAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CategoryDoesNotExist_ThrowsNotFoundException()
        {
            var seller = Seller();
            var categoryId = Guid.NewGuid();
            _userRepository.Setup(r => r.GetByIdAsync(seller.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seller);
            _categoryRepository.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            var command = new CreateAuctionCommand(
                "Title", "Description", null, 100m, DateTime.UtcNow.AddDays(1), categoryId, seller.Id, 1.00m);

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesAuctionWithMinimumIncrement()
        {
            var seller = Seller();
            var category = ValidCategory();
            _userRepository.Setup(r => r.GetByIdAsync(seller.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seller);
            _categoryRepository.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _mapper.Setup(m => m.Map<AuctionDto>(It.IsAny<Auction>()))
                .Returns(new AuctionDto(Guid.NewGuid(), "Title", "Description", null, 100m, 5m, 100m,
                    DateTime.UtcNow.AddDays(1), "Active", seller.Id, seller.Username, category.Id, category.Name, null));

            var command = new CreateAuctionCommand(
                "Title", "Description", null, 100m, DateTime.UtcNow.AddDays(1), category.Id, seller.Id, MinimumIncrement: 5m);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(5m, result.MinimumIncrement);
            _auctionRepository.Verify(r => r.AddAsync(
                It.Is<Auction>(a => a.MinimumIncrement == 5m && a.SellerId == seller.Id && a.CategoryId == category.Id),
                It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}