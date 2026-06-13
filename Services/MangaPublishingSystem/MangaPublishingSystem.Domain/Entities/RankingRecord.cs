using System;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class RankingRecord : BaseEntity
    {
        public int SeriesId { get; set; }
        public int VoteCount { get; set; }
        public int RankPosition { get; set; }
        public DateTime RecordedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Series Series { get; set; } = null!;
    }
}
