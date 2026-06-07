using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class TaskVersionConfiguration : IEntityTypeConfiguration<TaskVersion>
    {
        public void Configure(EntityTypeBuilder<TaskVersion> builder)
        {
            builder.ToTable("TaskVersion");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("VersionId");

            builder.Property(e => e.SubmittedFileUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Submitted")
                .IsRequired();

            builder.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            builder.HasOne(e => e.Task)
                .WithMany(t => t.TaskVersions)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.TaskId);
        }
    }
}
