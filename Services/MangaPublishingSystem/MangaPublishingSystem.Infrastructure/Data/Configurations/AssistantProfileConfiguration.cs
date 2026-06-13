using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class AssistantProfileConfiguration : IEntityTypeConfiguration<AssistantProfile>
    {
        public void Configure(EntityTypeBuilder<AssistantProfile> builder)
        {
            builder.ToTable("AssistantProfile");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("ProfileId");

            builder.Property(e => e.SpecialtyTags)
                .HasMaxLength(255);

            builder.Property(e => e.TotalCompletedTasks)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.OnTimeRate)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.DisputeRate)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.CurrentActiveTasks)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.AverageRating)
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.HasOne(e => e.Assistant)
                .WithOne(u => u.AssistantProfile)
                .HasForeignKey<AssistantProfile>(e => e.AssistantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.AssistantId)
                .IsUnique();
        }
    }
}
