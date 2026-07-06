namespace MangaPublishingSystem.Application.DTOs.AI
{
    public class BoundingBoxDto
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
