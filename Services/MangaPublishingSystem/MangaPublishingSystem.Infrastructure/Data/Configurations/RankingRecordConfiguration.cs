using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Infrastructure.Data.Configurations
{
    public class RankingRecordConfiguration : IEntityTypeConfiguration<RankingRecord>
    {
        public void Configure(EntityTypeBuilder<RankingRecord> builder)
        {
            builder.ToTable("RankingRecord");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("RankingId");

            builder.Property(e => e.VoteCount)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.RankPosition)
                .IsRequired();

            builder.Property(e => e.RecordedDate)
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            builder.HasOne(e => e.Series)
                .WithMany(s => s.RankingRecords)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.SeriesId);
        }
    }
}
