using System;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class DisputeLog : BaseEntity
    {
        public int EditorId { get; set; }
        public int TaskId { get; set; }
        public string? EditorComment { get; set; }
        public string ResolutionType { get; set; } = null!;
        public decimal? AssistantPercentage { get; set; }
        public decimal? MangakaPercentage { get; set; }
        public DateTime ResolvedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual User Editor { get; set; } = null!;
        public virtual Tasks Task { get; set; } = null!;
    }
}
