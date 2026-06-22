namespace MangaPublishingSystem.Application.DTOs.Regions
{
    public class UpdateRegionDto
    {
        public string CoordinatesJson { get; set; } = null!;
        public string? Name { get; set; }
    }
}
