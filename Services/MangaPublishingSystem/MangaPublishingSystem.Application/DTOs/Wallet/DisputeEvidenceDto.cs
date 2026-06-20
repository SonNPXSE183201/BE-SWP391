using System;

namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class DisputeEvidenceDto
    {
        public string SubmittedBy { get; set; } = null!;
        public string? SubmitterName { get; set; }
        public string Type { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
