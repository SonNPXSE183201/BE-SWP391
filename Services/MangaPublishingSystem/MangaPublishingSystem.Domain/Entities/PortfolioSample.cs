using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class PortfolioSample : BaseEntity
    {
        public int AssistantId { get; set; }
        public string Title { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string Category { get; set; } = null!;

        // Navigation property
        public virtual User Assistant { get; set; } = null!;
    }
}
