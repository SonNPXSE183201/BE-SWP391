using System;

namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class BoardVoteDto
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int BoardMemberId { get; set; }
        public string VoteType { get; set; } = null!;
        public decimal RecommendedBudget { get; set; }
        public string? Comment { get; set; }
        public DateTime VoteAt { get; set; }
    }
}
