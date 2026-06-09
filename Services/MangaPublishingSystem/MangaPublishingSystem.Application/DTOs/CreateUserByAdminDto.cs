namespace MangaPublishingSystem.Application.DTOs.User
{
    public class CreateUserByAdminDto
    {
        public int RoleId { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}