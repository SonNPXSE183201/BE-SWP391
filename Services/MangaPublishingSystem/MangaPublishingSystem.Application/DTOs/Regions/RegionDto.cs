using System;

namespace MangaPublishingSystem.Application.DTOs.Regions
{
    public class RegionDto
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public string? Name { get; set; }
        public string CoordinatesJson { get; set; } = null!;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
