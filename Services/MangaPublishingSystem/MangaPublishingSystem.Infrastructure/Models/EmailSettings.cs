namespace MangaPublishingSystem.Infrastructure.Models
{
    public class EmailSettings
    {
        public string DefaultFromEmail { get; set; } = null!;
        public string DefaultFromName { get; set; } = null!;
        public string SmtpServer { get; set; } = null!;
        public int Port { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool EnableSsl { get; set; }
    }
}
