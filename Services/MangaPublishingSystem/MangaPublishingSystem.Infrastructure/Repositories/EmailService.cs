using System.Net;
using System.Net.Mail;
using MangaPublishingSystem.Application.DTOs.User;
using MangaPublishingSystem.Application.IServices;
using Microsoft.Extensions.Options;

namespace MangaPublishingSystem.Infrastructure.Repositories;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAccountInfoAsync(string toEmail, string username, string password)
{
    using var mail = new MailMessage();

    mail.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
    mail.To.Add(toEmail);

    mail.Subject = "🎉 Your Manga Publishing Account";

    mail.Body = $@"
<div style='background:#eef2f7;padding:40px;font-family:Arial,sans-serif'>
    <div style='max-width:600px;margin:auto;background:white;border-radius:10px;overflow:hidden'>

        <div style='background:#16a34a;color:white;text-align:center;padding:30px'>
            <h1 style='margin:0'>🎉 Congratulations 🎉</h1>
            <p style='margin-top:10px'>Your account has been created successfully</p>
        </div>

        <div style='padding:30px'>
            <p>Hello,</p>

            <p>
               Your Manga Publishing System account has been created successfully.
            </p>

            <div style='background:#f3f4f6;padding:20px;border-radius:8px'>
                <p><strong>Username:</strong> {username}</p>
                <p><strong>Password:</strong> {password}</p>
            </div>

            <p style='color:red;font-weight:bold;margin-top:20px'>
                Please log in and change your password after your first login.
            </p>
        </div>

        

    </div>
</div>";

    mail.IsBodyHtml = true;

    using var client = new SmtpClient();

    client.Host = _settings.SmtpHost;
    client.Port = _settings.SmtpPort;
    client.EnableSsl = true;
    client.DeliveryMethod = SmtpDeliveryMethod.Network;
    client.UseDefaultCredentials = false;
    client.Credentials = new NetworkCredential(
        _settings.SenderEmail.Trim(),
        _settings.AppPassword.Trim()
    );

    await client.SendMailAsync(mail);
}
}