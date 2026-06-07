using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.ToTable("Region");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("RegionId");

            builder.Property(e => e.CoordinatesJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(e => e.Name)
                .HasMaxLength(100);

            builder.HasOne(e => e.Page)
                .WithMany(p => p.Regions)
                .HasForeignKey(e => e.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.PageId);
        }
    }
}
