namespace MangaPublishingSystem.Application.DTOs.User
{
    public class CreateUserByAdminDto
    {
        public int RoleId { get; set; }

        private string _userName = null!;
        public string UserName
        {
            get => _userName;
            set => _userName = (value ?? string.Empty).Trim().Replace(" ", "_");
        }

        private string _email = null!;
        public string Email
        {
            get => _email;
            set => _email = (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private string _fullName = null!;
        public string FullName
        {
            get => _fullName;
            set => _fullName = (value ?? string.Empty).Trim();
        }

        private string? _penName;
        public string? PenName
        {
            get => _penName;
            set => _penName = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
    }
}
