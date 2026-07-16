using System;

namespace MangaPublishingSystem.Application.DTOs.Publishing
{
    public class PublishingScheduleDto
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; } = null!;
        public int ChapterNumber { get; set; }
        public string ChapterTitle { get; set; } = null!;
        public string MangakaName { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public DateTime PublishDate { get; set; }
        public string Status { get; set; } = null!;
    }
}
