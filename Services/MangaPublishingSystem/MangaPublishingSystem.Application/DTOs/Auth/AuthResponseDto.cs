namespace MangaPublishingSystem.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? PenName { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
        public string? PhoneNumber { get; set; }
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
