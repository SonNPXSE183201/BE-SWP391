using System;
using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.Rankings
{
    public class CreateRankingsDto
    {
        public List<RankingInputDto> Records { get; set; } = new List<RankingInputDto>();
        public DateTime RecordedDate { get; set; }
    }
}
