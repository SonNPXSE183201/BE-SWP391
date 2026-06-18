namespace MangaPublishingSystem.Application.DTOs.Assistant
{
    public class AssistantResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? PenName { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
        
        // Assistant Profile specific properties
        public string? SpecialtyTags { get; set; }
        public int TotalCompletedTasks { get; set; }
        public decimal OnTimeRate { get; set; }
        public decimal DisputeRate { get; set; }
        public int CurrentActiveTasks { get; set; }
        public decimal AverageRating { get; set; }
    }
}
