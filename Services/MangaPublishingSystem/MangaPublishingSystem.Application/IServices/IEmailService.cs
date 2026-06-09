<<<<<<< HEAD
namespace MangaPublishingSystem.Application.IServices;

public interface IEmailService
{
    Task SendAccountInfoAsync(string toEmail, string username, string password);
}
=======
using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
>>>>>>> origin/dev
