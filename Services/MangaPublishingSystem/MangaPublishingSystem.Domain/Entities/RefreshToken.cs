using System;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
