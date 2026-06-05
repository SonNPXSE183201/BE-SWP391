using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MangaPublishingSystem.Presentation.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Unit of work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
