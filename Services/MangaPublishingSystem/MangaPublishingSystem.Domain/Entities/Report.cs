using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Report : BaseEntity
    {
        public int ReporterId { get; set; }
        public int ReportedUserId { get; set; }
        public string Reason { get; set; } = null!;
        public string Status { get; set; } = "Pending";

        // Navigation properties
        public virtual User Reporter { get; set; } = null!;
        public virtual User ReportedUser { get; set; } = null!;
    }
}
