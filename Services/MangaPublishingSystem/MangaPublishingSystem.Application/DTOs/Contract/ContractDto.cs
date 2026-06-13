using System;

namespace MangaPublishingSystem.Application.DTOs.Contract
{
    public class ContractDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SeriesId { get; set; }
        public decimal BaseGenkouryoPrice { get; set; }
        public DateTime? SignedDate { get; set; }
        public string Status { get; set; } = null!;
        public string? MangakaName { get; set; }
        public string? SeriesTitle { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
