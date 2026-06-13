using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transaction");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("TransactionId");

            builder.Property(e => e.Type)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.SetupFundAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.WithdrawableAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .IsRequired();

            builder.Property(e => e.ReferenceCode)
                .HasMaxLength(100)
                .IsUnicode(false);

            builder.Property(e => e.CreateAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.HasOne(e => e.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(e => e.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.FromUser)
                .WithMany(u => u.FromTransactions)
                .HasForeignKey(e => e.FromUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.ToUser)
                .WithMany(u => u.ToTransactions)
                .HasForeignKey(e => e.ToUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Thông tin ngân hàng (dùng cho giao dịch rút tiền)
            builder.Property(e => e.BankName)
                .HasMaxLength(100);

            builder.Property(e => e.BankAccountNumber)
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Property(e => e.BankAccountName)
                .HasMaxLength(200);

            builder.Property(e => e.AdminNote)
                .HasMaxLength(500);

            builder.HasIndex(e => e.WalletId);
            builder.HasIndex(e => e.FromUserId);
            builder.HasIndex(e => e.ToUserId);
        }
    }
}
