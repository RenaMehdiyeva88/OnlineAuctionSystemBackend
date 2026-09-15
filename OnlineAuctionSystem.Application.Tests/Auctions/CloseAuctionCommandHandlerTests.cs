using MediatR;
using Moq;
using OnlineAuctionSystem.Application.Auctions.Commands.CloseAuction;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using Xunit;

namespace OnlineAuctionSystem.Application.Tests.Auctions
{
    public class CloseAuctionCommandHandlerTests
    {
        private readonly Mock<IAuctionRepository> _auctionRepository = new();
        private readonly Mock<IBidRepository> _bidRepository = new();
        private readonly Mock<INotificationRepository> _notificationRepository = new();
        private readonly Mock<INotificationService> _notificationService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private readonly CloseAuctionCommandHandler _handler;

        public CloseAuctionCommandHandlerTests()
        {
            _handler = new CloseAuctionCommandHandler(
                _auctionRepository.Object,
                _bidRepository.Object,
                _notificationRepository.Object,
                _notificationService.Object,
                _unitOfWork.Object);
        }

        private static Auction ActiveAuction(Guid sellerId) => new()
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            Title = "Test Lot",
            Description = "Test",
            StartingPrice = 100m,
            Status = AuctionStatus.Active,
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        [Fact]
        public async Task Handle_ClosingSomeoneElsesAuction_ThrowsForbiddenException()
        {
            // The core IDOR regression test: [Authorize(Roles = "Seller")] alone
            // only proves the caller is *a* seller, not that they own *this*
            // auction. A different seller's ID must be rejected here.
            var actualOwner = Guid.NewGuid();
            var impersonator = Guid.NewGuid();
            var auction = ActiveAuction(actualOwner);

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);

            var command = new CloseAuctionCommand(auction.Id, impersonator);

            await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command, CancellationToken.None));

            _auctionRepository.Verify(r => r.Update(It.IsAny<Auction>()), Times.Never);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_OwnerClosesAuctionWithBids_DeterminesHighestBidderAsWinner()
        {
            var sellerId = Guid.NewGuid();
            var auction = ActiveAuction(sellerId);
            var winningBidderId = Guid.NewGuid();
            var winningBid = new Bid { Id = Guid.NewGuid(), AuctionId = auction.Id, BidderId = winningBidderId, Amount = 500m };

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(winningBid);

            var command = new CloseAuctionCommand(auction.Id, sellerId);

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(AuctionStatus.Closed, auction.Status);
            Assert.Equal(winningBidderId, auction.WinnerId);

            _notificationService.Verify(
                n => n.NotifyAuctionWonAsync(winningBidderId, auction.Id, 500m, It.IsAny<CancellationToken>()),
                Times.Once);
            _notificationService.Verify(
                n => n.NotifyAuctionClosedAsync(sellerId, auction.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_OwnerClosesAuctionWithNoBids_ClosesWithoutWinner()
        {
            var sellerId = Guid.NewGuid();
            var auction = ActiveAuction(sellerId);

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Bid?)null);

            var command = new CloseAuctionCommand(auction.Id, sellerId);

            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(AuctionStatus.Closed, auction.Status);
            Assert.Null(auction.WinnerId);

            _notificationService.Verify(
                n => n.NotifyAuctionWonAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _notificationService.Verify(
                n => n.NotifyAuctionClosedAsync(sellerId, auction.Id, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AlreadyClosedAuction_IsIdempotentAndDoesNothing()
        {
            var sellerId = Guid.NewGuid();
            var auction = ActiveAuction(sellerId);
            auction.Status = AuctionStatus.Closed;

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);

            var command = new CloseAuctionCommand(auction.Id, sellerId);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Unit.Value, result);
            _bidRepository.Verify(r => r.GetHighestBidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ClosingAuction_SavesBeforeSendingRealtimeNotifications()
        {
            // Regression test for the "SignalR before SaveChangesAsync" bug:
            // clients must never be told an auction closed/won before that's
            // actually durable in the database. We assert ordering by having
            // the SaveChangesAsync mock record when it was called and fail
            // the test if any notification fired first.
            var sellerId = Guid.NewGuid();
            var auction = ActiveAuction(sellerId);
            var winningBid = new Bid { Id = Guid.NewGuid(), AuctionId = auction.Id, BidderId = Guid.NewGuid(), Amount = 300m };

            var callOrder = new List<string>();

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(winningBid);
            _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("save"))
                .ReturnsAsync(1);
            _notificationService.Setup(n => n.NotifyAuctionWonAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("notify-won"))
                .Returns(Task.CompletedTask);
            _notificationService.Setup(n => n.NotifyAuctionClosedAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("notify-closed"))
                .Returns(Task.CompletedTask);

            var command = new CloseAuctionCommand(auction.Id, sellerId);
            await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(new[] { "save", "notify-won", "notify-closed" }, callOrder);
        }
    }
}