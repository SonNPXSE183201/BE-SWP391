namespace MangaPublishingSystem.Application.DTOs.Tasks
{
    public class CompositeLayerDto
    {
        public string OverlayUrl { get; set; } = null!;
        public int ZIndex { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
