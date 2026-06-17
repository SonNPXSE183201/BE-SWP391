namespace MangaPublishingSystem.Application.DTOs.Notifications
{
    public class TaskStatusChangedPayload
    {
        public int TaskId { get; set; }
        public string Status { get; set; } = null!;
        public string? Message { get; set; }
    }
}
