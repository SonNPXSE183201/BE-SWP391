using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MangaPublishingSystem.Presentation.Extensions
{
    public static class SwaggerAuthExtensions
    {
        public static IServiceCollection AddSwaggerAndAuth(this IServiceCollection services, IConfiguration config)
        {
            // Future configuration for JWT and Swagger can go here if needed.
            // For now, it mirrors the reference template's placeholder structure.
            return services;
        }
    }
}
