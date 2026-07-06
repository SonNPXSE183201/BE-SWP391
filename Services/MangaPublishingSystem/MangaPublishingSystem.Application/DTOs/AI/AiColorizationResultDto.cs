namespace MangaPublishingSystem.Application.DTOs.AI
{
    public class AiColorizationResultDto
    {
        public bool Success { get; set; }
        public string ColorizedImageUrl { get; set; }
        public string ErrorMessage { get; set; }
    }
}
