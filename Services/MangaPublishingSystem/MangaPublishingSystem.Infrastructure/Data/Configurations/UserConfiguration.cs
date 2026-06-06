using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("UserId");

            builder.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .IsRequired();

            builder.Property(e => e.CreateAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(e => e.PenName)
                .HasMaxLength(100);

            builder.Property(e => e.PortfolioUrl)
                .HasMaxLength(500);

            builder.Property(e => e.Skills)
                .HasMaxLength(500);

            builder.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.UserName)
                .IsUnique();

            builder.HasIndex(e => e.Email)
                .IsUnique();

            builder.HasIndex(e => e.RoleId);
        }
    }
}
