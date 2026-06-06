using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class DisputeLogConfiguration : IEntityTypeConfiguration<DisputeLog>
    {
        public void Configure(EntityTypeBuilder<DisputeLog> builder)
        {
            builder.ToTable("DisputeLog");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("DisputeId");

            builder.Property(e => e.EditorComment)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.ResolutionType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.AssistantPercentage)
                .HasColumnType("decimal(5,2)");

            builder.Property(e => e.MangakaPercentage)
                .HasColumnType("decimal(5,2)");

            builder.Property(e => e.ResolvedAt)
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            builder.HasOne(e => e.Editor)
                .WithMany(u => u.EditorDisputes)
                .HasForeignKey(e => e.EditorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Task)
                .WithMany(t => t.DisputeLogs)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.TaskId);
        }
    }
}
