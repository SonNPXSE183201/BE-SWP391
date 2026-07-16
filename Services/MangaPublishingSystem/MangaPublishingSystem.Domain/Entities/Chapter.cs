using System;
using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Chapter : BaseEntity
    {
        public int SeriesId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = null!;
        public int ValidPageCount { get; set; }
        public decimal AppliedGenkouryoPrice { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public DateTime? PublishDate { get; set; }
        public string? QcChecklistData { get; set; }
        public string Status { get; set; } = "Draft";

        // Navigation properties
        public virtual Series Series { get; set; } = null!;
        public virtual ICollection<Page> Pages { get; set; } = new List<Page>();
    }
}
