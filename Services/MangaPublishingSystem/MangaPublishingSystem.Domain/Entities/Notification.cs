using System;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public string Content { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool IsRead { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
