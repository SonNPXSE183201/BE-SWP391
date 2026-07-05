using System;
using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;
using MangaPublishingSystem.Domain.Enums;

namespace MangaPublishingSystem.Domain.Entities
{
    public class User : BaseEntity
    {
        public int RoleId { get; set; }
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public UserStatus Status { get; set; } = UserStatus.Pending;
        public string? PenName { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsOnLeave { get; set; } = false;
        public int? AssignedEditorId { get; set; }

        // Navigation properties
        public virtual User? AssignedEditor { get; set; }
        public virtual Role Role { get; set; } = null!;
        public virtual Wallet? Wallet { get; set; }
        public virtual AssistantProfile? AssistantProfile { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        
        public virtual ICollection<Series> MangakaSeries { get; set; } = new List<Series>();
        public virtual ICollection<Series> EditorSeries { get; set; } = new List<Series>();
        
        public virtual ICollection<BoardVote> BoardVotes { get; set; } = new List<BoardVote>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        
        public virtual ICollection<Tasks> MangakaTasks { get; set; } = new List<Tasks>();
        public virtual ICollection<Tasks> AssistantTasks { get; set; } = new List<Tasks>();
        
        public virtual ICollection<DisputeLog> EditorDisputes { get; set; } = new List<DisputeLog>();
        public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
        
        public virtual ICollection<Report> FiledReports { get; set; } = new List<Report>();
        public virtual ICollection<Report> ReceivedReports { get; set; } = new List<Report>();

        public virtual ICollection<Transaction> FromTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Transaction> ToTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<PortfolioSample> PortfolioSamples { get; set; } = new List<PortfolioSample>();
        public virtual ICollection<SeriesAssistant> SeriesAssistants { get; set; } = new List<SeriesAssistant>();
    }
}
