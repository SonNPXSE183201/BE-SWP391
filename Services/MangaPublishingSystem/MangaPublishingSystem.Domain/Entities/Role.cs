using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string RoleName { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
