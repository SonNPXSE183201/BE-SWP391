using System.Net;
using System.Net.Mail;
using System.Linq;
using MangaPublishingSystem.Application.Common.Security;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Infrastructure.Data;
using MangaPublishingSystem.Infrastructure.Models;
using MangaPublishingSystem.Infrastructure.Services;
using MangaPublishingSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MangaPublishingSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            // Database Connection
            services.AddDbContext<MangaPublishingDbContext>(options =>
                options.UseSqlServer(
                    config.GetConnectionString("DefaultConnection"),
                    sql => sql.CommandTimeout(120)));

            // Register Unit Of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Auto-register all repositories in Infrastructure layer using reflection
            var repoTypes = typeof(DependencyInjection).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository") && !t.Name.StartsWith("Generic"));

            foreach (var type in repoTypes)
            {
                var interfaceType = type.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{type.Name}");
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, type);
                }
            }

            // Email Configuration
            var emailSettings = new EmailSettings
            {
                DefaultFromEmail = config["EmailSettings:DefaultFromEmail"] ?? "",
                DefaultFromName = config["EmailSettings:DefaultFromName"] ?? "",
                SmtpServer = config["EmailSettings:SmtpServer"] ?? "",
                Port = int.TryParse(config["EmailSettings:Port"], out var port) ? port : 587,
                Username = config["EmailSettings:Username"] ?? "",
                Password = config["EmailSettings:Password"] ?? "",
                EnableSsl = bool.TryParse(config["EmailSettings:EnableSsl"], out var ssl) ? ssl : true
            };

            services.AddFluentEmail(emailSettings.DefaultFromEmail, emailSettings.DefaultFromName)
                .AddSmtpSender(new SmtpClient(emailSettings.SmtpServer)
                {
                    Port = emailSettings.Port,
                    Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password),
                    EnableSsl = emailSettings.EnableSsl
                });

            services.AddScoped<IEmailService, FluentEmailService>();
            services.AddScoped<IImageCompositor, ImageCompositor>();

            // Security services
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // VNPay Payment
            services.Configure<VnPaySettings>(config.GetSection("VnPay"));
            services.AddScoped<IVnPayService, VnPayService>();
            services.AddHttpContextAccessor();

            // Storage configuration & DI registration
            services.Configure<MinioSettings>(config.GetSection("StorageSettings:Minio"));
            var storageProvider = config["StorageSettings:Provider"] ?? "Local";
            if (storageProvider.Equals("Minio", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<IStorageService, MinioStorageService>();
            }
            else
            {
                services.AddScoped<IStorageService, LocalStorageService>();
            }

            return services;
        }
    }
}
