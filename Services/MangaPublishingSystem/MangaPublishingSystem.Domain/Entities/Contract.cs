using System;
using System.Collections.Generic;
using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    public class Contract : BaseEntity
    {
        public int UserId { get; set; }
        public int SeriesId { get; set; }
        public int? TemplateId { get; set; }
        public decimal BaseGenkouryoPrice { get; set; }
        public string? ContractFileUrl { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Status { get; set; } = "Pending";

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Series Series { get; set; } = null!;
        public virtual ContractTemplate? Template { get; set; }
        public virtual ICollection<ContractAddendum> ContractAddendums { get; set; } = new List<ContractAddendum>();
    }
}
