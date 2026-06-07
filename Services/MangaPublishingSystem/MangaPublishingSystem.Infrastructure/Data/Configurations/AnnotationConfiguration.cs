using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class AnnotationConfiguration : IEntityTypeConfiguration<Annotation>
    {
        public void Configure(EntityTypeBuilder<Annotation> builder)
        {
            builder.ToTable("Annotation");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("AnnotationId");

            builder.Property(e => e.CoordinatesJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(e => e.Comment)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(e => e.Type)
                .HasMaxLength(50);

            builder.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.Annotations)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Page)
                .WithMany(p => p.Annotations)
                .HasForeignKey(e => e.PageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.TaskVersion)
                .WithMany(tv => tv.Annotations)
                .HasForeignKey(e => e.TaskVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.CreatedByUserId);
            
            // Filtered index for clean nullable columns performance
            builder.HasIndex(e => e.PageId)
                .HasFilter("[PageId] IS NOT NULL");
                
            builder.HasIndex(e => e.TaskVersionId)
                .HasFilter("[TaskVersionId] IS NOT NULL");
        }
    }
}
