using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Role");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("RoleId");

            builder.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(e => e.RoleName)
                .IsUnique();
        }
    }
}
