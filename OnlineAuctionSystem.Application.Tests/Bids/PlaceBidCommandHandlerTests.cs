using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineAuctionSystem.Application.Bids.Commands.PlaceBid;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Contracts.Bids;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using Xunit;

namespace OnlineAuctionSystem.Application.Tests.Bids
{
    public class PlaceBidCommandHandlerTests
    {
        private readonly Mock<IAuctionRepository> _auctionRepository = new();
        private readonly Mock<IBidRepository> _bidRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<INotificationRepository> _notificationRepository = new();
        private readonly Mock<INotificationService> _notificationService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IDateTime> _dateTime = new();
        private readonly Mock<IMapper> _mapper = new();

        private readonly PlaceBidCommandHandler _handler;

        private static readonly DateTime FixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public PlaceBidCommandHandlerTests()
        {
            _dateTime.Setup(d => d.UtcNow).Returns(FixedNow);

            _handler = new PlaceBidCommandHandler(
                _auctionRepository.Object,
                _bidRepository.Object,
                _userRepository.Object,
                _notificationRepository.Object,
                _notificationService.Object,
                _unitOfWork.Object,
                _dateTime.Object,
                _mapper.Object);
        }

        private static Auction ActiveAuction(Guid sellerId, decimal startingPrice = 100m) => new()
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            Title = "Test Lot",
            Description = "Test",
            StartingPrice = startingPrice,
            Status = AuctionStatus.Active,
            EndTime = FixedNow.AddDays(1)
        };

        private static User Buyer(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Username = "buyer1",
            Email = "buyer1@test.com",
            PasswordHash = "hash",
            Role = UserRole.Buyer
        };

        [Fact]
        public async Task Handle_BidAtOrBelowCurrentHighest_ThrowsInvalidBidException()
        {
            var seller = Guid.NewGuid();
            var auction = ActiveAuction(seller, startingPrice: 100m);
            var buyer = Buyer();

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _userRepository.Setup(r => r.GetByIdAsync(buyer.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buyer);
            // Someone already bid 150 — our new bid of 120 is too low.
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Bid { Id = Guid.NewGuid(), AuctionId = auction.Id, BidderId = Guid.NewGuid(), Amount = 150m });

            var command = new PlaceBidCommand(auction.Id, buyer.Id, 120m);

            await Assert.ThrowsAsync<InvalidBidException>(() => _handler.Handle(command, CancellationToken.None));

            _bidRepository.Verify(r => r.AddAsync(It.IsAny<Bid>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_SellerBiddingOnOwnAuction_ThrowsForbiddenException()
        {
            var seller = Buyer(); // reusing factory; role doesn't matter for this check
            seller.Role = UserRole.Seller;
            var auction = ActiveAuction(seller.Id);

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _userRepository.Setup(r => r.GetByIdAsync(seller.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(seller);

            var command = new PlaceBidCommand(auction.Id, seller.Id, 200m);

            await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command, CancellationToken.None));

            _bidRepository.Verify(r => r.AddAsync(It.IsAny<Bid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_AuctionClosed_ThrowsAuctionClosedException()
        {
            var seller = Guid.NewGuid();
            var auction = ActiveAuction(seller);
            auction.Status = AuctionStatus.Closed;
            var buyer = Buyer();

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);

            var command = new PlaceBidCommand(auction.Id, buyer.Id, 200m);

            await Assert.ThrowsAsync<AuctionClosedException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ConcurrentBidCausesRowVersionConflict_ThrowsConflictException()
        {
            // Simulates the race condition scenario: between our read of the
            // current highest bid and our SaveChangesAsync, another request
            // updated the same auction row first. EF Core surfaces this as
            // DbUpdateConcurrencyException — the handler must translate that
            // into a client-friendly ConflictException, not let it bubble up
            // as an unhandled 500.
            var seller = Guid.NewGuid();
            var auction = ActiveAuction(seller, startingPrice: 100m);
            var buyer = Buyer();

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _userRepository.Setup(r => r.GetByIdAsync(buyer.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buyer);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Bid?)null);
            _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException("Row was modified by another transaction."));

            var command = new PlaceBidCommand(auction.Id, buyer.Id, 150m);

            await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidHigherBid_SavesAndReturnsBidDto()
        {
            var seller = Guid.NewGuid();
            var auction = ActiveAuction(seller, startingPrice: 100m);
            var buyer = Buyer();
            var expectedDto = new BidDto(Guid.NewGuid(), 150m, FixedNow, auction.Id, buyer.Id, buyer.Username);

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _userRepository.Setup(r => r.GetByIdAsync(buyer.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buyer);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Bid?)null);
            _mapper.Setup(m => m.Map<BidDto>(It.IsAny<Bid>())).Returns(expectedDto);

            var command = new PlaceBidCommand(auction.Id, buyer.Id, 150m);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(expectedDto, result);
            _bidRepository.Verify(r => r.AddAsync(It.Is<Bid>(b => b.Amount == 150m && b.BidderId == buyer.Id), It.IsAny<CancellationToken>()), Times.Once);
            _auctionRepository.Verify(r => r.Update(It.Is<Auction>(a => a.Id == auction.Id)), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            // No previous bid exists, so nobody should be notified as "outbid".
            _notificationService.Verify(
                n => n.NotifyOutbidAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_OutbidsAnotherUser_NotifiesPreviousBidder()
        {
            var seller = Guid.NewGuid();
            var auction = ActiveAuction(seller, startingPrice: 100m);
            var buyer = Buyer();
            var previousBidder = Guid.NewGuid();
            var previousBid = new Bid { Id = Guid.NewGuid(), AuctionId = auction.Id, BidderId = previousBidder, Amount = 120m };

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _userRepository.Setup(r => r.GetByIdAsync(buyer.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buyer);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(previousBid);
            _mapper.Setup(m => m.Map<BidDto>(It.IsAny<Bid>()))
                .Returns(new BidDto(Guid.NewGuid(), 150m, FixedNow, auction.Id, buyer.Id, buyer.Username));

            var command = new PlaceBidCommand(auction.Id, buyer.Id, 150m);

            await _handler.Handle(command, CancellationToken.None);

            _notificationService.Verify(
                n => n.NotifyOutbidAsync(previousBidder, auction.Id, 150m, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FirstBidEqualToStartingPrice_IsAccepted()
        {
            // Regression test for a mentor-flagged audit bug: the old code
            // compared (previousTopBid?.Amount ?? auction.StartingPrice) with
            // "<=" for BOTH the first-bid and subsequent-bid cases, which
            // meant a first bid exactly equal to the starting price
            // (e.g. StartingPrice=100, bid=100) was wrongly rejected — the
            // very first bidder had to overbid the starting price by at
            // least a cent. A first bid must succeed when it equals the
            // starting price exactly.
            var seller = Guid.NewGuid();
            var auction = ActiveAuction(seller, startingPrice: 100m);
            var buyer = Buyer();
            var expectedDto = new BidDto(Guid.NewGuid(), 100m, FixedNow, auction.Id, buyer.Id, buyer.Username);

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _userRepository.Setup(r => r.GetByIdAsync(buyer.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buyer);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Bid?)null);
            _mapper.Setup(m => m.Map<BidDto>(It.IsAny<Bid>())).Returns(expectedDto);

            // Bid exactly equal to StartingPrice (100) — must NOT throw.
            var command = new PlaceBidCommand(auction.Id, buyer.Id, 100m);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(expectedDto, result);
            _bidRepository.Verify(
                r => r.AddAsync(It.Is<Bid>(b => b.Amount == 100m), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_FirstBidBelowStartingPrice_ThrowsInvalidBidException()
        {
            var seller = Guid.NewGuid();
            var auction = ActiveAuction(seller, startingPrice: 100m);
            var buyer = Buyer();

            _auctionRepository.Setup(r => r.GetByIdAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
            _userRepository.Setup(r => r.GetByIdAsync(buyer.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buyer);
            _bidRepository.Setup(r => r.GetHighestBidAsync(auction.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Bid?)null);

            // Below StartingPrice (100) — must still be rejected.
            var command = new PlaceBidCommand(auction.Id, buyer.Id, 99m);

            await Assert.ThrowsAsync<InvalidBidException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}