using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Annotation : BaseEntity
    {
        public int CreatedByUserId { get; set; }
        public int? PageId { get; set; }
        public int? TaskVersionId { get; set; }
        public string CoordinatesJson { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public string? Type { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual Page? Page { get; set; }
        public virtual TaskVersion? TaskVersion { get; set; }
    }
}
