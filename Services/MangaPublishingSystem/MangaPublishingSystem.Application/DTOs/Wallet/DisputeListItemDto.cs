using System;

namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class DisputeListItemDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public string? SeriesTitle { get; set; }
        public string? MangakaName { get; set; }
        public string? AssistantName { get; set; }
        public decimal LockedAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }
    }
}
