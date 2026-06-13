using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallet");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("WalletId");

            builder.Property(e => e.SetupFundBalance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.WithdrawableBalance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.LockedFund)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.LockedWithdrawable)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.HasOne(e => e.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<Wallet>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.UserId)
                .IsUnique();
        }
    }
}
