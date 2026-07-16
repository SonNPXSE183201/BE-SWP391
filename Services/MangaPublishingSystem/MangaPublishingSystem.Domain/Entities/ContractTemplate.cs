using System;
using System.Collections.Generic;

namespace MangaPublishingSystem.Domain.Entities
{
    public class ContractTemplate : MangaPublishingSystem.Domain.Common.BaseEntity
    {
        public string Content { get; set; } = null!;
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public int CreatedByUserId { get; set; }

        public User CreatedByUser { get; set; } = null!;
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
