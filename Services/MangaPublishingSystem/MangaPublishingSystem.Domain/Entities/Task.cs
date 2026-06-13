using System;
using System.Collections.Generic;

namespace MangaPublishingSystem.Domain.Entities;

public partial class Task
{
    public int TaskId { get; set; }

    public int MangakaId { get; set; }

    public int RegionId { get; set; }

    public int? AssistantId { get; set; }

    public string? Description { get; set; }

    public decimal PaymentAmount { get; set; }

    public DateTime Deadline { get; set; }

    public int? ExtensionRequestDays { get; set; }

    public string? ExtensionReason { get; set; }

    public string? ExtensionStatus { get; set; }

    public int ZindexOrder { get; set; }

    public string Status { get; set; } = null!;

    public int? Rating { get; set; }

    public string? FeedbackComment { get; set; }

    public virtual User? Assistant { get; set; }

    public virtual ICollection<DisputeLog> DisputeLogs { get; set; } = new List<DisputeLog>();

    public virtual User Mangaka { get; set; } = null!;

    public virtual Region Region { get; set; } = null!;

    public virtual ICollection<TaskVersion> TaskVersions { get; set; } = new List<TaskVersion>();
}
