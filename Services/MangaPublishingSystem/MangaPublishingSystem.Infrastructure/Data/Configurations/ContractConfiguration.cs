using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.ToTable("Contract");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("ContractId");

            builder.Property(e => e.BaseGenkouryoPrice)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .IsRequired();

            builder.HasOne(e => e.User)
                .WithMany(u => u.Contracts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Series)
                .WithMany(s => s.Contracts)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.SeriesId);
        }
    }
}
