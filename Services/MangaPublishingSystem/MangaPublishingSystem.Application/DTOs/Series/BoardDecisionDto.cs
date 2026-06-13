namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class BoardDecisionDto
    {
        public bool IsApproved { get; set; }
        public decimal ApprovedProductionBudget { get; set; }
        public string PublicationSchedule { get; set; } = null!; // "Weekly" or "Monthly"
    }
}
