using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notification");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("NotId");

            builder.Property(e => e.Content)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(e => e.Type)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(e => e.CreateAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.UserId);
        }
    }
}
