using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class AssistantProfile : BaseEntity
    {
        public int AssistantId { get; set; }
        public string? SpecialtyTags { get; set; }
        public int TotalCompletedTasks { get; set; }
        public decimal OnTimeRate { get; set; }
        public decimal DisputeRate { get; set; }
        public int CurrentActiveTasks { get; set; }
        public decimal AverageRating { get; set; }

        // Navigation properties
        public virtual User Assistant { get; set; } = null!;
    }
}
