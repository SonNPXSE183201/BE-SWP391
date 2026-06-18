namespace MangaPublishingSystem.Application.DTOs.Regions
{
    public class CreateRegionDto
    {
        public int PageId { get; set; }
        public string Name { get; set; } = null!;
        public string CoordinatesJson { get; set; } = null!;
    }
}
