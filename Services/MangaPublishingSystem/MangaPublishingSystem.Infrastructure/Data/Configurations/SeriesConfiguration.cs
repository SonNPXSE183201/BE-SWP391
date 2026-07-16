using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class SeriesConfiguration : IEntityTypeConfiguration<Series>
    {
        public void Configure(EntityTypeBuilder<Series> builder)
        {
            builder.ToTable("Series");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("SeriesId");

            builder.Property(e => e.Title)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(e => e.Genre)
                .HasMaxLength(100);

            builder.Property(e => e.Synopsis)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.CoverArtworkUrl)
                .HasMaxLength(500);

            builder.Property(e => e.EstimatedProductionBudget)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.EditorRecommendedBudget)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.ApprovedProductionBudget)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.PublicationSchedule)
                .HasMaxLength(50);

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Draft")
                .IsRequired();

            builder.Property(e => e.ResourceFolderUrl)
                .HasMaxLength(500);

            builder.Property(e => e.EditorNote)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.MangakaSubmissionNote)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.ContractRejectionCount)
                .HasDefaultValue(0)
                .IsRequired();

            builder.HasOne(e => e.Mangaka)
                .WithMany(u => u.MangakaSeries)
                .HasForeignKey(e => e.MangakaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Editor)
                .WithMany(u => u.EditorSeries)
                .HasForeignKey(e => e.EditorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.MangakaId);
            builder.HasIndex(e => e.EditorId);
            builder.HasIndex(e => e.Status);
        }
    }
}
