namespace MangaPublishingSystem.Application.DTOs.Rankings
{
    public class VoteRankingRequestDto
    {
        public int SeriesId { get; set; }
        public string VoteType { get; set; } = null!;
        public string? Comment { get; set; }
    }
}
