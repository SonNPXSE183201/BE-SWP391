using System;

namespace MangaPublishingSystem.Application.DTOs.Tasks
{
    public class TaskVersionDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int VersionNumber { get; set; }
        public string SubmittedFileUrl { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
    }
}
