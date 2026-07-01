using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.Chapters
{
    public class ChapterProductionCheckDto
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? Detail { get; set; }
    }

    public class ChapterProductionReadinessDto
    {
        public int ChapterId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool CanSubmit { get; set; }
        public int TotalPages { get; set; }
        public int PagesReady { get; set; }
        public int OpenTaskCount { get; set; }
        public List<ChapterProductionCheckDto> Checks { get; set; } = new();
        public List<string> Blockers { get; set; } = new();
    }
}
