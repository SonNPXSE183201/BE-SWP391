using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.AI
{
    public class AiTagsResultDto
    {
        public bool Success { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
