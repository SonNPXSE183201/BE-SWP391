namespace MangaPublishingSystem.Application.DTOs.Auth
{
    public class RegisterDto
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
        public string? VerificationCode { get; set; }
    }
}
