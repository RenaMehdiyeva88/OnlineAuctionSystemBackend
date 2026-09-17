using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(c => c.Name).IsUnique();

            builder.HasMany(c => c.Auctions)
                .WithOne(a => a.Category)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete: an admin "deleting" a category just flips IsDeleted,
            // never a real DELETE (existing auctions still reference the row).
            // This filter makes every query against Categories automatically
            // exclude soft-deleted rows, with zero changes needed in any
            // repository or handler that queries categories.
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
