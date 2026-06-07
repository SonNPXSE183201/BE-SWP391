namespace MangaPublishingSystem.Application.DTOs.User
{
    public class UserResponseDto
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}