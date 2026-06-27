using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.Reviews
{
    public class RequireSeriesRevisionDto
    {
        public string Comment { get; set; } = null!;
        public decimal? SuggestedBudget { get; set; }
        public List<string>? FailedChecklistItems { get; set; }
    }
}
