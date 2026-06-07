using System;
using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class TaskVersion : BaseEntity
    {
        public int TaskId { get; set; }
        public int VersionNumber { get; set; }
        public string SubmittedFileUrl { get; set; } = null!;
        public string Status { get; set; } = "Submitted";
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Tasks Task { get; set; } = null!;
        public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
    }
}
