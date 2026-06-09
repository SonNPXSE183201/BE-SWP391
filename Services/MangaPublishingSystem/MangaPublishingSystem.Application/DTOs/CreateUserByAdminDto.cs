namespace MangaPublishingSystem.Application.DTOs.User
{
    public class CreateUserByAdminDto
    {
        public int RoleId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;

        public string? PenName { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
    }
}