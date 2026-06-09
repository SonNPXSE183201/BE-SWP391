using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices.Auth
{
    public interface IOtpService
    {
        Task SendOtpAsync(string email);
        Task SendForgotPasswordOtpAsync(string email);
        bool VerifyOtp(string email, string code);
    }
}
