namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class CreateSeriesDto
    {
        public string Title { get; set; } = null!;
        public string? Genre { get; set; }
        public string? Synopsis { get; set; }
        public string? CoverArtworkUrl { get; set; }
        public string? DraftManuscriptUrl { get; set; }
        public decimal EstimatedProductionBudget { get; set; }
    }
}
