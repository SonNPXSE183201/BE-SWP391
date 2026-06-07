using System;
using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Tasks : BaseEntity
    {
        public int MangakaId { get; set; }
        public int RegionId { get; set; }
        public int? AssistantId { get; set; }
        public string? Description { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime Deadline { get; set; }
        public int? ExtensionRequestDays { get; set; }
        public string? ExtensionReason { get; set; }
        public string? ExtensionStatus { get; set; } = "None";
        public int ZIndex_Order { get; set; }
        public string Status { get; set; } = "Draft";
        public int? Rating { get; set; }
        public string? FeedbackComment { get; set; }

        // Navigation properties
        public virtual User Mangaka { get; set; } = null!;
        public virtual Region Region { get; set; } = null!;
        public virtual User? Assistant { get; set; }
        
        public virtual ICollection<TaskVersion> TaskVersions { get; set; } = new List<TaskVersion>();
        public virtual ICollection<DisputeLog> DisputeLogs { get; set; } = new List<DisputeLog>();
    }
}
