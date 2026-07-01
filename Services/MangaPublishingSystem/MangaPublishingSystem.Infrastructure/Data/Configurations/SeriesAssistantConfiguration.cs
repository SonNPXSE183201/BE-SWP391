using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class SeriesAssistantConfiguration : IEntityTypeConfiguration<SeriesAssistant>
    {
        public void Configure(EntityTypeBuilder<SeriesAssistant> builder)
        {
            builder.ToTable("Series_Assistant");

            builder.HasKey(e => new { e.SeriesId, e.AssistantId });

            builder.Property(e => e.RoleInTeam)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Pending");

            builder.Property(e => e.CreateAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.HasOne(e => e.Series)
                .WithMany(s => s.SeriesAssistants)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Assistant)
                .WithMany(u => u.SeriesAssistants)
                .HasForeignKey(e => e.AssistantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.AssistantId);
            builder.HasIndex(e => new { e.SeriesId, e.Status });
        }
    }
}
