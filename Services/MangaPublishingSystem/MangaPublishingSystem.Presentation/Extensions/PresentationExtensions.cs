using Microsoft.Extensions.DependencyInjection;

namespace MangaPublishingSystem.Presentation.Extensions
{
    public static class PresentationExtensions
    {
        public static IServiceCollection AddPresentationServices(this IServiceCollection services)
        {
            // Configure controllers with JSON options to ignore cycles and preserve naming policies
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                });

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
