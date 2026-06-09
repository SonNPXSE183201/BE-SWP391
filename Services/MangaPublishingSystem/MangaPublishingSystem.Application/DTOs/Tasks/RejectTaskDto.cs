namespace MangaPublishingSystem.Application.DTOs.Tasks
{
    public class RejectTaskDto
    {
        public string FeedbackComment { get; set; } = null!;
        public int RevisionExtensionHours { get; set; } // +24 or +48 as per T02
        public string CoordinatesJson { get; set; } = null!;
    }
}
