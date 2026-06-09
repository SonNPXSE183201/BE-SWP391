using System;

namespace MangaPublishingSystem.Application.DTOs.Tasks
{
    public class TasksDto
    {
        public int Id { get; set; }
        public int MangakaId { get; set; }
        public int RegionId { get; set; }
        public int? AssistantId { get; set; }
        public string? Description { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime Deadline { get; set; }
        public int? ExtensionRequestDays { get; set; }
        public string? ExtensionReason { get; set; }
        public string? ExtensionStatus { get; set; }
        public int ZIndex_Order { get; set; }
        public string Status { get; set; } = null!;
        public int? Rating { get; set; }
        public string? FeedbackComment { get; set; }
        
        public string? MangakaName { get; set; }
        public string? AssistantName { get; set; }
        public int PageNumber { get; set; }
        public string? PageImageUrl { get; set; }

        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
