using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices.Auth
{
    public interface IOtpService
    {
        Task SendOtpAsync(string email);
        bool VerifyOtp(string email, string code);
    }
}
