namespace MangaPublishingSystem.Application.DTOs.Annotations
{
    public class CreateAnnotationDto
    {
        public int? PageId { get; set; }
        public int? TaskVersionId { get; set; }
        public string CoordinatesJson { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public string? Type { get; set; }
    }
}
