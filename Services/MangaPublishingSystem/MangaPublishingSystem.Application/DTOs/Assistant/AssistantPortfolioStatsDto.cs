namespace MangaPublishingSystem.Application.DTOs.Assistant
{
    public class AssistantPortfolioStatsDto
    {
        public int TotalCompletedTasks { get; set; }
        public decimal OnTimeRate { get; set; }
        public decimal DisputeRate { get; set; }
        public int CurrentActiveTasks { get; set; }
        public decimal AverageRating { get; set; }
        public decimal TotalEarnings { get; set; }
    }
}
