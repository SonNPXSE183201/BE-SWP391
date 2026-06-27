using Microsoft.Extensions.DependencyInjection;

namespace MangaPublishingSystem.Presentation.Extensions
{
    public static class PresentationExtensions
    {
        public static IServiceCollection AddPresentationServices(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                });

            services.AddHttpContextAccessor();

            services.AddScoped<
                MangaPublishingSystem.Application.IServices.INotificationPublisher,
                Services.NotificationPublisher>();

            // Đã bật lại để hỗ trợ tính năng tự động chốt sổ
            services.AddHostedService<Services.TaskAutomationBackgroundService>();

            return services;
        }
    }
}