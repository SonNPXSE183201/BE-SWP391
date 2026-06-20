using System;

namespace MangaPublishingSystem.Application.DTOs.Reviews
{
    public class ChapterSummaryDto
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int PageCount { get; set; }
    }
}
