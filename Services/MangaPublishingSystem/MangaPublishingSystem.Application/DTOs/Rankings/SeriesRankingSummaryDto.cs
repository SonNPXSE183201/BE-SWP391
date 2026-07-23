using System;

namespace MangaPublishingSystem.Application.DTOs.Rankings
{
    public class SeriesRankingSummaryDto
    {
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; } = string.Empty;
        public int CurrentRank { get; set; }
        public int TotalVotes { get; set; }
        public DateTime RecordedDate { get; set; }
        public bool IsBottomTier { get; set; }
    }
}
