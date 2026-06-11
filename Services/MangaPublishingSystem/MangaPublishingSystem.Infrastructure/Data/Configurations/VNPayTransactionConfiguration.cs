using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class VNPayTransactionConfiguration : IEntityTypeConfiguration<VNPayTransaction>
    {
        public void Configure(EntityTypeBuilder<VNPayTransaction> builder)
        {
            builder.ToTable("VNPayTransaction", t =>
            {
                t.HasCheckConstraint(
                    "CK_VNPayTransaction_Type",
                    "Type IN (N'Deposit', N'Withdrawal')");
                t.HasCheckConstraint(
                    "CK_VNPayTransaction_Status",
                    "Status IN (N'Pending', N'Success', N'Failed')");
            });

            builder.HasKey(e => e.Id);

            builder.Property(e => e.TransactionId)
                .IsRequired();

            builder.HasIndex(e => e.TransactionId)
                .IsUnique();

            builder.Property(e => e.WalletId)
                .IsRequired();

            builder.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Lưu Enum dưới dạng chuỗi theo GEMINI.md
            builder.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.ReferenceCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.HasIndex(e => e.ReferenceCode)
                .IsUnique();

            builder.Property(e => e.CreateAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();
        }
    }
}
