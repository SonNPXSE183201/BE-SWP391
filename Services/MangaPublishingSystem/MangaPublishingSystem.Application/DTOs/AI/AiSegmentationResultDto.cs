using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.AI
{
    public class AiSegmentationResultDto
    {
        public bool Success { get; set; }
        public List<BoundingBoxDto> Panels { get; set; } = new();
    }
}
