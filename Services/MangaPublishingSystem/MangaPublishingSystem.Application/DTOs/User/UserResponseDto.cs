namespace MangaPublishingSystem.Application.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int RoleId { get; set; }
        public string Status { get; set; } = null!;
        public string? PenName { get; set; }
        public string? Message { get; set; }
    }
}
