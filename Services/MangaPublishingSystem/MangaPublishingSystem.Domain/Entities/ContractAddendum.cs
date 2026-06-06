using System;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class ContractAddendum : BaseEntity
    {
        public int ContractId { get; set; }
        public decimal NewGenkouryoPrice { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? SignedDate { get; set; }

        // Navigation properties
        public virtual Contract Contract { get; set; } = null!;
    }
}
