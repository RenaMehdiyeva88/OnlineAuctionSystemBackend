namespace OnlineAuctionSystem.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Soft delete flag. Not every entity uses this (only ones where a
        // hard DELETE would break FK/audit history — currently Category),
        // but it lives on the base so any entity can opt in later just by
        // adding a HasQueryFilter in its EF Core configuration.
        public bool IsDeleted { get; set; }
    }
}
