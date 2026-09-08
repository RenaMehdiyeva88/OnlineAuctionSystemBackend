using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineAuctionSystem.Domain.Entities;

namespace OnlineAuctionSystem.Persistence.Configurations
{
    public class NotificationConfiguration
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.IsRead)
                .HasDefaultValue(false);

            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(n => n.Auction)
                .WithMany()
                .HasForeignKey(n => n.AuctionId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasIndex(n => new { n.UserId, n.IsRead });
        }
    }
}
