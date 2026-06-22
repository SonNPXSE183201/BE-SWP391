using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class PortfolioSampleConfiguration : IEntityTypeConfiguration<PortfolioSample>
    {
        public void Configure(EntityTypeBuilder<PortfolioSample> builder)
        {
            builder.ToTable("PortfolioSample");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("SampleId");

            builder.Property(e => e.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Category)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(e => e.Assistant)
                .WithMany(u => u.PortfolioSamples)
                .HasForeignKey(e => e.AssistantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.AssistantId);
        }
    }
}
