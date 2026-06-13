namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class EditorEvaluationDto
    {
        public bool IsApproved { get; set; }
        public string EvaluationReport { get; set; } = null!;
        public decimal SuggestedBudget { get; set; }
    }
}
