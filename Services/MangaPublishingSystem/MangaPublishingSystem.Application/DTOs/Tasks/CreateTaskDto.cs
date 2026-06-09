using System;

namespace MangaPublishingSystem.Application.DTOs.Tasks
{
    public class CreateTaskDto
    {
        public int RegionId { get; set; }
        public int? AssistantId { get; set; }
        public string? Description { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime Deadline { get; set; }
        public int ZIndex_Order { get; set; }
    }
}
