using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Persistence.Configurations
{
    public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
    {
        public void Configure(EntityTypeBuilder<Auction> builder)
        {
            builder.HasKey(a => a.Id);

            // F2 — title, description, starting price, end time.
            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(a => a.ImageUrl)
                .HasMaxLength(2000);

            builder.Property(a => a.StartingPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(a => a.EndTime)
                .IsRequired();

            // F4/F5 — auction lifecycle status.
            builder.Property(a => a.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.HasOne(a => a.Seller)
                .WithMany(u => u.Auctions)
                .HasForeignKey(a => a.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // F5 — winner is optional until the auction closes.
            builder.HasOne(a => a.Winner)
                .WithMany()
                .HasForeignKey(a => a.WinnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // F8 — category-based browsing.
            builder.HasOne(a => a.Category)
                .WithMany(c => c.Auctions)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // F7 — bid history.
            builder.HasMany(a => a.Bids)
                .WithOne(b => b.Auction)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            // CurrentHighestBid / IsExpired are computed properties, not mapped columns.
            builder.Ignore(a => a.CurrentHighestBid);
            builder.Ignore(a => a.IsExpired);

            // F4 — speeds up the background job's poll for expired active auctions.
            builder.HasIndex(a => new { a.Status, a.EndTime });

            // F8 — speeds up category + price-range browsing/search.
            builder.HasIndex(a => a.CategoryId);
        }
    }
}
