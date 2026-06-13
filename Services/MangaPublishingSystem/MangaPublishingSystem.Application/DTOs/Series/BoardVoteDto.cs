namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class BoardVoteDto
    {
        public string VoteType { get; set; } = null!; // "Approved" or "Rejected"
        public decimal RecommendedBudget { get; set; }
        public string? Comment { get; set; }
    }
}
