namespace MangaPublishingSystem.Application.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public bool RequiresVerification { get; set; }
        public string Message { get; set; } = null!;
    }
}
