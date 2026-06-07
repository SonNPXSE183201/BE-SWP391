using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
