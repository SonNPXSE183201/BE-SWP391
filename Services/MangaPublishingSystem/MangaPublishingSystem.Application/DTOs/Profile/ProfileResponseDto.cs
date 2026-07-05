namespace MangaPublishingSystem.Application.DTOs.Profile
{
    public class ProfileResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public string? PenName { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
        public string? SpecialtyTags { get; set; }
    }
}
