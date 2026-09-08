using Microsoft.EntityFrameworkCore;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Domain.Entities;
using OnlineAuctionSystem.Persistence.Context;

namespace OnlineAuctionSystem.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuctionDbContext _context;

        public UserRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) =>
            await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
            await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

        public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
            await _context.Users.AddAsync(user, cancellationToken);

        public void Update(User user) => _context.Users.Update(user);
    }

}
