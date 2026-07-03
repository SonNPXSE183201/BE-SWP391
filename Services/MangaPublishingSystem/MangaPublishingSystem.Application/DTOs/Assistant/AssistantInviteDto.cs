using System;

namespace MangaPublishingSystem.Application.DTOs.Assistant
{
    public class AssistantInviteDto
    {
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public string RoleInTeam { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreateAt { get; set; }

        // Series detail
        public string? Genre { get; set; }
        public string? Synopsis { get; set; }
        public string? PublicationSchedule { get; set; }
        public string? SeriesStatus { get; set; }

        // Mangaka info
        public string? MangakaName { get; set; }
        public string? MangakaPenName { get; set; }

        // Team info
        public int TeamSize { get; set; }
    }
}
