using System;
using System.Threading.Tasks;
using FluentEmail.Core;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Infrastructure.Services
{
    public class FluentEmailService : IEmailService
    {
        private readonly IFluentEmail _fluentEmail;

        public FluentEmailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var response = await _fluentEmail
                    .To(toEmail)
                    .Subject(subject)
                    .Body(body, isHtml: true)
                    .SendAsync();

                if (!response.Successful)
                {
                    Console.WriteLine($"\n[WARNING] FluentEmail failed to send to {toEmail}: {string.Join(", ", response.ErrorMessages)}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[WARNING] FluentEmail exception while sending to {toEmail}: {ex.Message}\n");
            }
        }

        public async Task SendAccountInfoAsync(string toEmail, string userName, string password)
        {
            var subject = "Tài khoản Manga Publishing System đã được tạo";

            var body = $@"
<div style='background:#eef2f7;padding:40px;font-family:Arial,sans-serif'>
    <div style='max-width:600px;margin:auto;background:white;border-radius:10px;overflow:hidden'>

        <div style='background:#16a34a;color:white;text-align:center;padding:30px'>
            <h1 style='margin:0'>Tài khoản đã được tạo thành công</h1>
            <p style='margin-top:10px'>
                Quản trị viên đã khởi tạo tài khoản cho bạn trên hệ thống.
            </p>
        </div>

        <div style='padding:30px'>

            <p>Xin chào,</p>

            <p>
                Tài khoản của bạn trên hệ thống Manga Publishing System đã được tạo thành công.
            </p>

            <div style='background:#f3f4f6;
                        padding:20px;
                        border-radius:8px;
                        margin-top:20px'>

                <p>
                    <strong>Tên đăng nhập:</strong> {userName}
                </p>

                <p>
                    <strong>Mật khẩu:</strong> {password}
                </p>

            </div>

            <p style='color:red;
                      font-weight:bold;
                      margin-top:25px'>

                Vui lòng đăng nhập và đổi mật khẩu ngay sau lần đăng nhập đầu tiên.

            </p>

        </div>

        <div style='text-align:center;
                    padding:15px;
                    color:#666'>

            © Manga Publishing System

        </div>

    </div>
</div>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}