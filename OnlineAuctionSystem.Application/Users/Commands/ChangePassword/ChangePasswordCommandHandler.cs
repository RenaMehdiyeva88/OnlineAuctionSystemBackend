using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;

namespace OnlineAuctionSystem.Application.Users.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandHandler
        : IRequestHandler<ChangePasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(
            ChangePasswordCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (userId == null)
            {
                throw new UnauthorizedException(
                    "User is not authenticated.");
            }

            var user = await _userRepository.GetByIdAsync(
                userId.Value,
                cancellationToken);

            if (user == null)
            {
                throw new NotFoundException(
                    "User",
                    userId.Value);
            }

            if (!_passwordHasher.Verify(
                    request.CurrentPassword,
                    user.PasswordHash))
            {
                throw new UnauthorizedException(
                    "Current password is incorrect.");
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                throw new ConflictException(
                    "New password must be different from the current password.");
            }

            user.PasswordHash =
                _passwordHasher.Hash(request.NewPassword);

            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiryTime = null;

            _userRepository.Update(user);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
    }
}