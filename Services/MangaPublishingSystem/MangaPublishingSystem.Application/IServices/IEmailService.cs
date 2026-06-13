namespace MangaPublishingSystem.Application.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);

        Task SendAccountInfoAsync(string toEmail, string username, string password);
    }
}