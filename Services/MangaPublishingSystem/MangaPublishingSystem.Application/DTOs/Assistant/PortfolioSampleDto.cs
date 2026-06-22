using System;

namespace MangaPublishingSystem.Application.DTOs.Assistant
{
    public class PortfolioSampleDto
    {
        public int Id { get; set; }
        public int AssistantId { get; set; }
        public string Title { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string Category { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
