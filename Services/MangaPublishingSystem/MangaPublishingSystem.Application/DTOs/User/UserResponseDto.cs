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
        public bool IsOnLeave { get; set; }
        public int? AssignedEditorId { get; set; }
        public string? AssignedEditorName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Message { get; set; }
    }
}
