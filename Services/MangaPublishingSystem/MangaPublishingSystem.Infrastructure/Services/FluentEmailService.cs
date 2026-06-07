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

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var response = await _fluentEmail
                .To(to)
                .Subject(subject)
                .Body(body, isHtml: true)
                .SendAsync();

            if (!response.Successful)
            {
                throw new System.Exception(string.Join(", ", response.ErrorMessages));
            }
        }
    }
}
