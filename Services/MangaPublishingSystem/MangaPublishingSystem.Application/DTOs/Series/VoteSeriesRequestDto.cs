namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class VoteSeriesRequestDto
    {
        public bool Approved { get; set; }
        public string? VoteChoice { get; set; }
        public string Comment { get; set; } = null!;
        public decimal RecommendedBudget { get; set; }
        public string? PublicationSchedule { get; set; }
    }
}
