namespace MangaPublishingSystem.Application.DTOs.Auth
{
    public class RefreshTokenDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
