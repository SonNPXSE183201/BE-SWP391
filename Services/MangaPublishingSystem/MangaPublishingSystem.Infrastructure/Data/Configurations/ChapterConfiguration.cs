using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
    {
        public void Configure(EntityTypeBuilder<Chapter> builder)
        {
            builder.ToTable("Chapter");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("ChapterId");

            builder.Property(e => e.ChapterNumber)
                .IsRequired();

            builder.Property(e => e.Title)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(e => e.ValidPageCount)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.AppliedGenkouryoPrice)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.QcChecklistData)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Draft")
                .IsRequired();

            builder.HasOne(e => e.Series)
                .WithMany(s => s.Chapters)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.SeriesId);
            builder.HasIndex(e => e.Status);
        }
    }
}
