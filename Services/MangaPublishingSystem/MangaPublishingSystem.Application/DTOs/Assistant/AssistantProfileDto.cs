namespace MangaPublishingSystem.Application.DTOs.Assistant
{
    public class AssistantProfileDto
    {
        public int AssistantId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PenName { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
        public string? SpecialtyTags { get; set; }
        public int TotalCompletedTasks { get; set; }
        public decimal OnTimeRate { get; set; }
        public decimal DisputeRate { get; set; }
        public int CurrentActiveTasks { get; set; }
        public decimal AverageRating { get; set; }
    }
}
