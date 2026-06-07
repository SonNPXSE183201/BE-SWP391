using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class ContractAddendumConfiguration : IEntityTypeConfiguration<ContractAddendum>
    {
        public void Configure(EntityTypeBuilder<ContractAddendum> builder)
        {
            builder.ToTable("ContractAddendum");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("AddendumId");

            builder.Property(e => e.NewGenkouryoPrice)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.EffectiveDate)
                .IsRequired();

            builder.HasOne(e => e.Contract)
                .WithMany(c => c.ContractAddendums)
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
