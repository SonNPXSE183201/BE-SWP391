using System;

namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class SeriesDto
    {
        public int Id { get; set; }
        public int MangakaId { get; set; }
        public int? EditorId { get; set; }
        public string Title { get; set; } = null!;
        public string? Genre { get; set; }
        public string? Synopsis { get; set; }
        public string? CoverArtworkUrl { get; set; }
        public decimal EstimatedProductionBudget { get; set; }
        public decimal ApprovedProductionBudget { get; set; }
        public string? PublicationSchedule { get; set; }
        public string Status { get; set; } = null!;
        public string? ResourceFolderUrl { get; set; }
        public string? MangakaName { get; set; }
        public string? EditorName { get; set; }
        public string? EditorNote { get; set; }
        public string? MangakaSubmissionNote { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public System.Collections.Generic.ICollection<BoardVoteDto>? BoardVotes { get; set; }
        public bool HasContract { get; set; }
    }
}
