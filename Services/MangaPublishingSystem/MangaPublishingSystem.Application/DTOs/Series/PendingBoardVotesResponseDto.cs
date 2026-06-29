using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class PendingBoardVotesResponseDto
    {
        public BoardVotingRulesDto Rules { get; set; } = new();
        public IEnumerable<SeriesDto> Series { get; set; } = new List<SeriesDto>();
    }
}
