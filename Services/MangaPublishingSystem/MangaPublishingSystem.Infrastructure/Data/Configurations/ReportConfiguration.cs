using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Report");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("ReportId");

            builder.Property(e => e.Reason)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .IsRequired();

            builder.HasOne(e => e.Reporter)
                .WithMany(u => u.FiledReports)
                .HasForeignKey(e => e.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.ReportedUser)
                .WithMany(u => u.ReceivedReports)
                .HasForeignKey(e => e.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.ReporterId);
            builder.HasIndex(e => e.ReportedUserId);
        }
    }
}
