using AutoMapper;
using MediatR;
using OnlineAuctionSystem.Application.Common.Exceptions;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Contracts.Users;
using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Application.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.UserId);

            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase)
                && await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            {
                throw new ConflictException("A user with this email already exists.");
            }

            user.Username = request.Username;
            user.Email = request.Email;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<UserDto>(user);
        }
    }
}
