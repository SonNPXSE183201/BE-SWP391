using System;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class BoardVote : BaseEntity
    {
        public int SeriesId { get; set; }
        public int BoardMemberId { get; set; }
        public string VoteType { get; set; } = null!;
        public decimal RecommendedBudget { get; set; }
        public string? PublicationSchedule { get; set; }
        public string? Comment { get; set; }
        public DateTime VoteAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Series Series { get; set; } = null!;
        public virtual User BoardMember { get; set; } = null!;
    }
}
