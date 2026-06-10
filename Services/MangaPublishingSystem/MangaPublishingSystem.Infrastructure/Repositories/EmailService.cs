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

        Console.WriteLine("===== EMAIL SETTINGS =====");
        Console.WriteLine($"SenderEmail : '{_settings.SenderEmail}'");
        Console.WriteLine($"SenderName  : '{_settings.SenderName}'");
        Console.WriteLine($"SmtpHost    : '{_settings.SmtpHost}'");
        Console.WriteLine($"SmtpPort    : '{_settings.SmtpPort}'");
        Console.WriteLine($"AppPassword : '{_settings.AppPassword}'");
        Console.WriteLine("==========================");
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var mail = new MailMessage();

        mail.From = new MailAddress(
            _settings.SenderEmail,
            _settings.SenderName);

        mail.To.Add(to);

        mail.Subject = subject;
        mail.Body = body;
        mail.IsBodyHtml = true;

        using var client = CreateSmtpClient();

        await client.SendMailAsync(mail);
    }

    public async Task SendAccountInfoAsync(
        string toEmail,
        string username,
        string password)
    {
        var subject = "Your Manga Publishing Account";

        var body = $@"
<h2>Congratulations!</h2>

<p>Your account has been created successfully.</p>

<p><strong>Username:</strong> {username}</p>
<p><strong>Password:</strong> {password}</p>

<p>Please change your password after first login.</p>";

        await SendEmailAsync(toEmail, subject, body);
    }

    private SmtpClient CreateSmtpClient()
    {
        return new SmtpClient
        {
            Host = _settings.SmtpHost,
            Port = _settings.SmtpPort,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _settings.SenderEmail.Trim(),
                _settings.AppPassword.Trim())
        };
    }
}