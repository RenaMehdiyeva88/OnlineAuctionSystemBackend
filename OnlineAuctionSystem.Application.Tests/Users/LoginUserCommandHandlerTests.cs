using Moq;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Application.Users.Commands.LoginUser;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Domain.Enums;
using Xunit;

namespace OnlineAuctionSystem.Application.Tests.Users
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IDateTime> _dateTime = new();

        private static readonly DateTime FixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        private readonly LoginUserCommandHandler _handler;

        public LoginUserCommandHandlerTests()
        {
            _dateTime.Setup(d => d.UtcNow).Returns(FixedNow);
            _handler = new LoginUserCommandHandler(
                _userRepository.Object,
                _passwordHasher.Object,
                _tokenService.Object,
                _unitOfWork.Object,
                _dateTime.Object);
        }

        private static User ExistingUser() => new()
        {
            Id = Guid.NewGuid(),
            Username = "buyer1",
            Email = "buyer1@test.com",
            PasswordHash = "correct-hash",
            Role = UserRole.Buyer
        };

        [Fact]
        public async Task Handle_EmailNotFound_ThrowsUnauthorizedException()
        {
            _userRepository.Setup(r => r.GetByEmailAsync("missing@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new LoginUserCommand("missing@test.com", "whatever");

            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WrongPassword_ThrowsUnauthorizedException()
        {
            var user = ExistingUser();
            _userRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasher.Setup(p => p.Verify("wrong-password", user.PasswordHash)).Returns(false);

            var command = new LoginUserCommand(user.Email, "wrong-password");

            await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));

            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_ReturnsTokensAndUpdatesRefreshToken()
        {
            var user = ExistingUser();
            _userRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _passwordHasher.Setup(p => p.Verify("correct-password", user.PasswordHash)).Returns(true);
            _tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
            _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");

            var command = new LoginUserCommand(user.Email, "correct-password");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal("refresh-token", user.RefreshToken);
            Assert.Equal(FixedNow.AddDays(7), user.RefreshTokenExpiryTime);

            _userRepository.Verify(r => r.Update(user), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}