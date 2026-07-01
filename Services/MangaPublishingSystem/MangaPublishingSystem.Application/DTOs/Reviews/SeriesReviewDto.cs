using System;
using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.Reviews
{
    public class SeriesReviewDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Genre { get; set; }
        public string? Synopsis { get; set; }
        public string? CoverArtworkUrl { get; set; }
        public string? ResourceFolderUrl { get; set; }
        public decimal EstimatedProductionBudget { get; set; }
        public decimal EditorRecommendedBudget { get; set; }
        public decimal ApprovedProductionBudget { get; set; }
        public string Status { get; set; } = null!;
        public int MangakaId { get; set; }
        public string? MangakaName { get; set; }
        public int? EditorId { get; set; }
        public string? EditorName { get; set; }
        public string? EditorNote { get; set; }
        public string? MangakaSubmissionNote { get; set; }
        public int ChapterCount { get; set; }
        public List<ChapterSummaryDto> Chapters { get; set; } = new List<ChapterSummaryDto>();
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
