using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.IServices.Auth;
using MangaPublishingSystem.Application.Common.Templates;

namespace MangaPublishingSystem.Application.Services.Auth
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public OtpService(IMemoryCache cache, IEmailService emailService)
        {
            _cache = cache;
            _emailService = emailService;
        }

        public async Task SendOtpAsync(string email)
        {
            // Tạo mã OTP ngẫu nhiên 6 chữ số
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Lưu OTP vào cache với thời hạn 5 phút
            var cacheKey = $"otp:{email}";
            _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

            // Log OTP ra console để hỗ trợ dev test nhanh mà không cần check mail
            Console.WriteLine($"\n>>> [DEBUG OTP] Mã OTP cho email {email} là: {otpCode}\n");

            // Gửi email chứa mã OTP
            var subject = "Mã xác thực đăng ký tài khoản Trợ lý";
            var body = EmailTemplates.GetOtpEmailBody(otpCode);

            await _emailService.SendEmailAsync(email, subject, body);
        }

        public async Task SendForgotPasswordOtpAsync(string email)
        {
            // Tạo mã OTP ngẫu nhiên 6 chữ số
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();

            // Lưu OTP vào cache với thời hạn 5 phút
            var cacheKey = $"otp:{email}";
            _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

            // Log OTP ra console để hỗ trợ dev test nhanh mà không cần check mail
            Console.WriteLine($"\n>>> [DEBUG OTP - QUÊN MẬT KHẨU] Mã OTP cho email {email} là: {otpCode}\n");

            // Gửi email chứa mã OTP quên mật khẩu
            var subject = "Mã xác thực đặt lại mật khẩu";
            var body = EmailTemplates.GetForgotPasswordOtpEmailBody(otpCode);

            await _emailService.SendEmailAsync(email, subject, body);
        }

        public bool VerifyOtp(string email, string code)
        {
            var cacheKey = $"otp:{email}";
            if (_cache.TryGetValue(cacheKey, out string? cachedCode))
            {
                if (cachedCode == code)
                {
                    _cache.Remove(cacheKey); // Xóa mã OTP sau khi xác thực thành công
                    return true;
                }
            }
            return false;
        }
    }
}
