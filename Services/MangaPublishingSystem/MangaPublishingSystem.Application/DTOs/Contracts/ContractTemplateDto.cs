using System;

namespace MangaPublishingSystem.Application.DTOs.Contracts
{
    public class ContractTemplateDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
