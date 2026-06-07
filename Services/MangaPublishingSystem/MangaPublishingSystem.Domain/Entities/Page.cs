using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Page : BaseEntity
    {
        public int ChapterId { get; set; }
        public int PageNumber { get; set; }
        public string RawImageUrl { get; set; } = null!;
        public string? CompositeImageUrl { get; set; }
        public string? BaseLayerUrl { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsApproved { get; set; }

        // Navigation properties
        public virtual Chapter Chapter { get; set; } = null!;
        public virtual ICollection<Region> Regions { get; set; } = new List<Region>();
        public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
    }
}
