using System;
using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class DisputeDetailDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public string? SeriesTitle { get; set; }
        public string? ChapterTitle { get; set; }
        public string? MangakaName { get; set; }
        public string? AssistantName { get; set; }
        public decimal LockedAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? Resolution { get; set; }
        public DateTime TaskDeadline { get; set; }
        public DateTime? TaskSubmittedAt { get; set; }
        public string? RegionInfo { get; set; }
        public string? MangakaReason { get; set; }
        public string? AssistantReason { get; set; }
        public List<DisputeEvidenceDto> Evidence { get; set; } = new List<DisputeEvidenceDto>();
    }
}
