using System;

namespace MangaPublishingSystem.Application.DTOs.Rankings
{
    public class RankingHistoryRecordDto
    {
        public DateTime RecordedDate { get; set; }
        public int RankPosition { get; set; }
        public int VoteCount { get; set; }
    }
}
