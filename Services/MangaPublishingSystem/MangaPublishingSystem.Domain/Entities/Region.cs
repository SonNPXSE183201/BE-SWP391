using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Region : BaseEntity
    {
        public int PageId { get; set; }
        public string CoordinatesJson { get; set; } = null!;
        public string? Name { get; set; }

        // Navigation properties
        public virtual Page Page { get; set; } = null!;
        public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
    }
}
