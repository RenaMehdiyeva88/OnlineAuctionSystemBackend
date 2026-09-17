using AutoMapper;
using Moq;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Application.Users.Commands.RegisterUser;
using OnlineAuctionSystem.Domain.Entities;
using Xunit;

namespace OnlineAuctionSystem.Application.Tests.Users
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IDateTime> _dateTime = new();
        private readonly Mock<IMapper> _mapper = new();

        private static readonly DateTime FixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        private readonly RegisterUserCommandHandler _handler;

        public RegisterUserCommandHandlerTests()
        {
            _dateTime.Setup(d => d.UtcNow).Returns(FixedNow);
            _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
            _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("fake-access-token");
            _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("fake-refresh-token");

            _handler = new RegisterUserCommandHandler(
                _userRepository.Object,
                _passwordHasher.Object,
                _tokenService.Object,
                _unitOfWork.Object,
                _dateTime.Object,
                _mapper.Object);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ThrowsConflictException()
        {
            _userRepository.Setup(r => r.EmailExistsAsync("taken@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new RegisterUserCommand("newuser", "taken@test.com", "Password123!", "Buyer");

            await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));

            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NewEmail_CreatesUserAndReturnsTokens()
        {
            _userRepository.Setup(r => r.EmailExistsAsync("new@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new RegisterUserCommand("newuser", "new@test.com", "Password123!", "Buyer");

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("newuser", result.Username);
            Assert.Equal("Buyer", result.Role);
            Assert.Equal("fake-access-token", result.AccessToken);
            Assert.Equal("fake-refresh-token", result.RefreshToken);

            _userRepository.Verify(r => r.AddAsync(
                It.Is<User>(u => u.Email == "new@test.com" && u.PasswordHash == "hashed-password"),
                It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}