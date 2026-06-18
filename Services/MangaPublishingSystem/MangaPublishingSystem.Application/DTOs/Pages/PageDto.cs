using System;

namespace MangaPublishingSystem.Application.DTOs.Pages
{
    public class PageDto
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public int PageNumber { get; set; }
        public string RawImageUrl { get; set; } = null!;
        public string? CompositeImageUrl { get; set; }
        public string? BaseLayerUrl { get; set; }
        public string Status { get; set; } = null!;
        public bool IsApproved { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
}
