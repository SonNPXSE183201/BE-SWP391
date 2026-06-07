using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace MangaPublishingSystem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Auto-register all services in Application layer using reflection
            var serviceTypes = typeof(DependencyInjection).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service") && !t.Name.StartsWith("Generic"));

            foreach (var type in serviceTypes)
            {
                var interfaceType = type.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{type.Name}");
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, type);
                }
            }

            return services;
        }
    }
}
