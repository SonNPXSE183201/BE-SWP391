using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshToken");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("RefreshTokenId");

            builder.Property(e => e.Token)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.ExpiresAt)
                .IsRequired();

            builder.Property(e => e.IsRevoked)
                .HasDefaultValue(false)
                .IsRequired();

            builder.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.Token)
                .IsUnique();

            builder.HasIndex(e => e.UserId);
        }
    }
}
