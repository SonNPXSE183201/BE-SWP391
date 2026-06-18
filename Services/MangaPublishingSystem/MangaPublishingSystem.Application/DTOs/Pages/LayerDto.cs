namespace MangaPublishingSystem.Application.DTOs.Pages
{
    public class LayerDto
    {
        public int RegionId { get; set; }
        public string? RegionName { get; set; }
        public string CoordinatesJson { get; set; } = null!;
        public int ZIndex_Order { get; set; }
        public string? ImageUrl { get; set; }
        public string TaskStatus { get; set; } = "None";
    }
}
