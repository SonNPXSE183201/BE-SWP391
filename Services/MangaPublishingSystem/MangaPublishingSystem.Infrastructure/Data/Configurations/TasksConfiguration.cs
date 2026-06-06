using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class TasksConfiguration : IEntityTypeConfiguration<Tasks>
    {
        public void Configure(EntityTypeBuilder<Tasks> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("TaskId");

            builder.Property(e => e.Description)
                .HasMaxLength(1000);

            builder.Property(e => e.PaymentAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.Deadline)
                .IsRequired();

            builder.Property(e => e.ExtensionReason)
                .HasMaxLength(500);

            builder.Property(e => e.ExtensionStatus)
                .HasMaxLength(50)
                .HasDefaultValue("None");

            builder.Property(e => e.ZIndex_Order)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Draft")
                .IsRequired();

            builder.Property(e => e.FeedbackComment)
                .HasMaxLength(1000);

            builder.HasOne(e => e.Mangaka)
                .WithMany(u => u.MangakaTasks)
                .HasForeignKey(e => e.MangakaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Region)
                .WithMany(r => r.Tasks)
                .HasForeignKey(e => e.RegionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Assistant)
                .WithMany(u => u.AssistantTasks)
                .HasForeignKey(e => e.AssistantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.MangakaId);
            builder.HasIndex(e => e.AssistantId);
            builder.HasIndex(e => e.Status);
        }
    }
}
