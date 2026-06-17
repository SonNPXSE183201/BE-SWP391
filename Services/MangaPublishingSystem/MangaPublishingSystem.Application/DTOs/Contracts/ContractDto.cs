using System;

namespace MangaPublishingSystem.Application.DTOs.Contracts
{
    public class ContractDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? MangakaName { get; set; }
        public int SeriesId { get; set; }
        public string? SeriesTitle { get; set; }
        public decimal BaseGenkouryoPrice { get; set; }
        public DateTime? SignedDate { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreateAt { get; set; }
    }
}
