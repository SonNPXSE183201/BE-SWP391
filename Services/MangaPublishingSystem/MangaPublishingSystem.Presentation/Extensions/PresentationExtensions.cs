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
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                });

            services.AddHttpContextAccessor();

            services.AddScoped<
                MangaPublishingSystem.Application.IServices.INotificationPublisher,
                Services.NotificationPublisher>();

            // Tạm tắt vì DB hiện chưa có bảng RefreshToken
            // services.AddHostedService<Services.TaskAutomationBackgroundService>();

            return services;
        }
    }
}