using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class PageConfiguration : IEntityTypeConfiguration<Page>
    {
        public void Configure(EntityTypeBuilder<Page> builder)
        {
            builder.ToTable("Page");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("PageId");

            builder.Property(e => e.PageNumber)
                .IsRequired();

            builder.Property(e => e.RawImageUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.CompositeImageUrl)
                .HasMaxLength(500);

            builder.Property(e => e.BaseLayerUrl)
                .HasMaxLength(500);

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .IsRequired();

            builder.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasOne(e => e.Chapter)
                .WithMany(c => c.Pages)
                .HasForeignKey(e => e.ChapterId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.ChapterId);
            builder.HasIndex(e => e.Status);
        }
    }
}
