using System;

namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class SeriesAssistantDto
    {
        public int SeriesId { get; set; }
        public int AssistantId { get; set; }
        public string? AssistantName { get; set; }
        public string? AssistantEmail { get; set; }
        public string RoleInTeam { get; set; } = null!;
        public DateTime? JoinedDate { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreateAt { get; set; }
    }
}
