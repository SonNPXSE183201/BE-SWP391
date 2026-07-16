using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Series : BaseEntity
    {
        public int MangakaId { get; set; }
        public int? EditorId { get; set; }
        public string? EditorNote { get; set; }
        public string? MangakaSubmissionNote { get; set; }
        public string Title { get; set; } = null!;
        public string? Genre { get; set; }
        public string? Synopsis { get; set; }
        public string? CoverArtworkUrl { get; set; }
        public decimal EstimatedProductionBudget { get; set; }
        public decimal EditorRecommendedBudget { get; set; }
        public decimal ApprovedProductionBudget { get; set; }
        public string? PublicationSchedule { get; set; }
        public string Status { get; set; } = "Draft";
        public string? ResourceFolderUrl { get; set; }
        public int ContractRejectionCount { get; set; } = 0;

        // Navigation properties
        public virtual User Mangaka { get; set; } = null!;
        public virtual User? Editor { get; set; }
        public virtual ICollection<RankingRecord> RankingRecords { get; set; } = new List<RankingRecord>();
        public virtual ICollection<BoardVote> BoardVotes { get; set; } = new List<BoardVote>();
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public virtual ICollection<SeriesAssistant> SeriesAssistants { get; set; } = new List<SeriesAssistant>();
    }
}
