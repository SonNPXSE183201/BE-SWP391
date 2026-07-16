using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class BoardVoteConfiguration : IEntityTypeConfiguration<BoardVote>
    {
        public void Configure(EntityTypeBuilder<BoardVote> builder)
        {
            builder.ToTable("BoardVote");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("VoteId");

            builder.Property(e => e.VoteType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.RecommendedBudget)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0.00m)
                .IsRequired();

            builder.Property(e => e.PublicationSchedule)
                .HasMaxLength(50);

            builder.Property(e => e.Comment)
                .HasMaxLength(1000);

            builder.Property(e => e.VoteAt)
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            builder.HasOne(e => e.Series)
                .WithMany(s => s.BoardVotes)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.BoardMember)
                .WithMany(u => u.BoardVotes)
                .HasForeignKey(e => e.BoardMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => e.SeriesId);
        }
    }
}
