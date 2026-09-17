using Moq;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Users.Commands.RefreshToken;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using Xunit;

namespace OnlineAuctionSystem.Application.Tests.Users
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IDateTime> _dateTime = new();

        private static readonly DateTime FixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTests()
        {
            _dateTime.Setup(d => d.UtcNow).Returns(FixedNow);
            _handler = new RefreshTokenCommandHandler(
                _userRepository.Object, _tokenService.Object, _unitOfWork.Object, _dateTime.Object);
        }

        private static User ValidUser(Guid id, DateTime? refreshExpiry) => new()
        {
            Id = id,
            Username = "buyer1",
            Email = "buyer1@test.com",
            PasswordHash = "hash",
            Role = UserRole.Buyer,
            RefreshToken = "old-refresh-token",
            RefreshTokenExpiryTime = refreshExpiry
        };

        [Fact]
        public async Task Handle_InvalidAccessToken_ThrowsUnauthorizedException()
        {
            _tokenService.Setup(t => t.ValidateAccessTokenAndGetUserId("bad-token")).Returns((Guid?)null);

            var command = new RefreshTokenCommand("bad-token", "some-refresh-token");

            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

            _userRepository.Verify(r => r.GetByRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_RefreshTokenNotFound_ThrowsUnauthorizedException()
        {
            var userId = Guid.NewGuid();
            _tokenService.Setup(t => t.ValidateAccessTokenAndGetUserId("valid-token")).Returns(userId);
            _userRepository.Setup(r => r.GetByRefreshTokenAsync("unknown-refresh", It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new RefreshTokenCommand("valid-token", "unknown-refresh");

            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ExpiredRefreshToken_ThrowsUnauthorizedException()
        {
            var userId = Guid.NewGuid();
            var user = ValidUser(userId, FixedNow.AddDays(-1)); // expired yesterday

            _tokenService.Setup(t => t.ValidateAccessTokenAndGetUserId("valid-token")).Returns(userId);
            _userRepository.Setup(r => r.GetByRefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var command = new RefreshTokenCommand("valid-token", "old-refresh-token");

            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_TokenBelongsToDifferentUser_ThrowsUnauthorizedException()
        {
            var tokenUserId = Guid.NewGuid();
            var actualOwner = ValidUser(Guid.NewGuid(), FixedNow.AddDays(1));

            _tokenService.Setup(t => t.ValidateAccessTokenAndGetUserId("valid-token")).Returns(tokenUserId);
            _userRepository.Setup(r => r.GetByRefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(actualOwner);

            var command = new RefreshTokenCommand("valid-token", "old-refresh-token");

            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidRefreshToken_IssuesNewTokens()
        {
            var userId = Guid.NewGuid();
            var user = ValidUser(userId, FixedNow.AddDays(1));

            _tokenService.Setup(t => t.ValidateAccessTokenAndGetUserId("valid-token")).Returns(userId);
            _userRepository.Setup(r => r.GetByRefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("new-access-token");
            _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh-token");

            var command = new RefreshTokenCommand("valid-token", "old-refresh-token");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("new-access-token", result.AccessToken);
            Assert.Equal("new-refresh-token", result.RefreshToken);
            Assert.Equal("new-refresh-token", user.RefreshToken);

            _userRepository.Verify(r => r.Update(user), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}