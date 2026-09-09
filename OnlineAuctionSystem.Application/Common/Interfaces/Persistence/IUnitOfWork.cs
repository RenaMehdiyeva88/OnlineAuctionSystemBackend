namespace OnlineAuctionSystem.Application.Common.Interfaces.Persistence
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // A single SaveChangesAsync() call is already atomic (EF Core wraps it
        // in an implicit transaction), so most handlers never need these —
        // they're here for the rare use case that spans multiple explicit
        // SaveChangesAsync calls and needs them to succeed or fail together.
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
