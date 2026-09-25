using Microsoft.EntityFrameworkCore;
using MediatR;
using Moq;
using OnlineAuctionSystem.Application.Auctions.Commands.CancelAuction;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using Xunit;

namespace OnlineAuctionSystem.Application.Tests.Auctions
{
    public class CancelAuctionCommandHandlerTests
    {
        private readonly Mock<IAuctionRepository> _auctionRepository = new();
        private readonly Mock<IBidRepository> _bidRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private readonly CancelAuctionCommandHandler _handler;

        public CancelAuctionCommandHandlerTests()
        {
            _handler = new CancelAuctionCommandHandler(_auctionRepository.Object, _bidRepository.Object, _unitOfWork.Object);
        }

        private static Auction ActiveAuction(Guid sellerId, AuctionStatus status = AuctionStatus.Active) => new()
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            Title = "Test Lot",
            Description = "Test",
            StartingPrice = 100m,
            Status = status,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        [Fact]
        public async Task Handle_NotOwner_ThrowsForbiddenException()
        {
            var actualOwner = Guid.NewGuid();
            var impersonator = Guid.NewGuid();
            var auction = ActiveAuction(actualOwner);
            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(auction);

            var command = new CancelAuctionCommand(auction.Id, impersonator);

            await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command, CancellationToken.None));
            _auctionRepository.Verify(r => r.Update(It.IsAny<Auction>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AlreadyClosed_IsIdempotentAndDoesNothing()
        {
            var sellerId = Guid.NewGuid();
            var auction = ActiveAuction(sellerId, AuctionStatus.Closed);
            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(auction);

            var command = new CancelAuctionCommand(auction.Id, sellerId);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            _bidRepository.Verify(r => r.GetHighestBidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AuctionAlreadyHasBids_ThrowsForbiddenException()
        {
            var sellerId = Guid.NewGuid();
            var auction = ActiveAuction(sellerId);
            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(auction);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Bid { Id = Guid.NewGuid(), AuctionId = auction.Id, BidderId = Guid.NewGuid(), Amount = 150m });

            var command = new CancelAuctionCommand(auction.Id, sellerId);

            await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command, CancellationToken.None));
            _auctionRepository.Verify(r => r.Update(It.IsAny<Auction>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ValidRequest_SetsStatusToCancelled()
        {
            var sellerId = Guid.NewGuid();
            var auction = ActiveAuction(sellerId);
            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(auction);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Bid?)null);

            var command = new CancelAuctionCommand(auction.Id, sellerId);

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(AuctionStatus.Cancelled, auction.Status);
            _auctionRepository.Verify(r => r.Update(auction), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ConcurrencyConflict_ThrowsConflictException()
        {
            var sellerId = Guid.NewGuid();
            var auction = ActiveAuction(sellerId);
            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>())).ReturnsAsync(auction);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Bid?)null);
            _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException("Row modified concurrently."));

            var command = new CancelAuctionCommand(auction.Id, sellerId);

            await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}