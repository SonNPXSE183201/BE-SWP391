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
            var response = await _fluentEmail
                .To(toEmail)
                .Subject(subject)
                .Body(body, isHtml: true)
                .SendAsync();

            if (!response.Successful)
            {
                throw new Exception(string.Join(", ", response.ErrorMessages));
            }
        }

        public async Task SendAccountInfoAsync(string toEmail, string userName, string password)
        {
            var subject = "🎉 Your Manga Publishing Account";

            var body = $@"
<div style='background:#eef2f7;padding:40px;font-family:Arial,sans-serif'>
    <div style='max-width:600px;margin:auto;background:white;border-radius:10px;overflow:hidden'>

        <div style='background:#16a34a;color:white;text-align:center;padding:30px'>
            <h1 style='margin:0'>🎉 CONGRATULATIONS!</h1>
            <p style='margin-top:10px'>
                Your account has been created successfully.
            </p>
        </div>

        <div style='padding:30px'>

            <p>Hello,</p>

            <p>
                Your Manga Publishing System account has been created successfully.
            </p>

            <div style='background:#f3f4f6;
                        padding:20px;
                        border-radius:8px;
                        margin-top:20px'>

                <p>
                    <strong>Username:</strong> {userName}
                </p>

                <p>
                    <strong>Password:</strong> {password}
                </p>

            </div>

            <p style='color:red;
                      font-weight:bold;
                      margin-top:25px'>

                Please log in and change your password
                after your first login.

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