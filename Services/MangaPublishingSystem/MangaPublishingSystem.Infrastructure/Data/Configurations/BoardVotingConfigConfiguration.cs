using MangaPublishingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class BoardVotingConfigConfiguration : IEntityTypeConfiguration<BoardVotingConfig>
    {
        public void Configure(EntityTypeBuilder<BoardVotingConfig> builder)
        {
            builder.ToTable("BoardVotingConfig");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).HasColumnName("ConfigId");

            builder.Property(c => c.TiePolicy).HasMaxLength(32).IsRequired().HasDefaultValue("Escalate");
            builder.Property(c => c.AutoResolveHours).HasDefaultValue(48);
            builder.Property(c => c.ApprovalThresholdPercent).HasDefaultValue(66);
            builder.Property(c => c.RejectionThresholdPercent).HasDefaultValue(66);
            builder.Property(c => c.ClearVotesOnResubmit).HasDefaultValue(true);
            builder.Property(c => c.RequireOddBoardSize).HasDefaultValue(true);
            builder.Property(c => c.BoardRoleId).HasDefaultValue(3);
        }
    }
}
