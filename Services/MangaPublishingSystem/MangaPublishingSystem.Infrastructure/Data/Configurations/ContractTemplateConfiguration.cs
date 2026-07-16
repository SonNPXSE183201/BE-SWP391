using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class ContractTemplateConfiguration : IEntityTypeConfiguration<ContractTemplate>
    {
        public void Configure(EntityTypeBuilder<ContractTemplate> builder)
        {
            builder.ToTable("ContractTemplate");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasColumnName("TemplateId");

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.ContractTemplates)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
