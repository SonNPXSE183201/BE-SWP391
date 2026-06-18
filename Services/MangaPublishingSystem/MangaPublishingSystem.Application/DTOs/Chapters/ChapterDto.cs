using System;

namespace MangaPublishingSystem.Application.DTOs.Chapters
{
    public class ChapterDto
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = null!;
        public int ValidPageCount { get; set; }
        public decimal AppliedGenkouryoPrice { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public string? QcChecklistData { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
