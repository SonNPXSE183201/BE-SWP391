using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsFromConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var allowAny = bool.TryParse(configuration["Cors:AllowAnyOrigin"], out var any) && any;
            var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new string[0];

            services.AddCors(options =>
            {
                options.AddPolicy("Default", builder =>
                {
                    if (allowAny)
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                    else
                    {
                        builder.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
                    }
                });
            });

            return services;
        }
    }
}
