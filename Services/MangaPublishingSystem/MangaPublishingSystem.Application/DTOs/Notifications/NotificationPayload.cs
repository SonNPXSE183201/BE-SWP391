using System;

namespace MangaPublishingSystem.Application.DTOs.Notifications
{
    public class NotificationPayload
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Link { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreateAt { get; set; }
    }
}
