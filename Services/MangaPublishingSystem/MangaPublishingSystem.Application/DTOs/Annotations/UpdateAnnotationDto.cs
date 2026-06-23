namespace MangaPublishingSystem.Application.DTOs.Annotations
{
    public class UpdateAnnotationDto
    {
        public string CoordinatesJson { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public string? Type { get; set; }
    }
}
