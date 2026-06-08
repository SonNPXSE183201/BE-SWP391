namespace MangaPublishingSystem.Application.IServices;

public interface IEmailService
{
    Task SendAccountInfoAsync(string toEmail, string username, string password);
}