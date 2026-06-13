using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace BuildingBlocks.Web
{
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Adds basic self health check
        /// </summary>
        public static IServiceCollection AddBasicHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "ready", "live" });
            return services;
        }

        /// <summary>
        /// Adds SQL Server database health check
        /// </summary>
        public static IServiceCollection AddSqlServerHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(conn))
            {
                services.AddHealthChecks()
                    .AddSqlServer(
                        connectionString: conn,
                        name: "sql-database",
                        failureStatus: HealthStatus.Unhealthy,
                        tags: new[] { "db", "sql", "ready" });
            }
            return services;
        }

        /// <summary>
        /// Adds Redis health check
        /// </summary>
        public static IServiceCollection AddRedisHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnection = configuration["Redis:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(redisConnection))
            {
                services.AddHealthChecks()
                    .AddRedis(
                        redisConnectionString: redisConnection,
                        name: "redis",
                        failureStatus: HealthStatus.Unhealthy,
                        tags: new[] { "cache", "redis", "ready" });
            }
            return services;
        }

        /// <summary>
        /// Adds all health checks (self, database, redis)
        /// </summary>
        public static IServiceCollection AddAllHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBasicHealthChecks();
            services.AddSqlServerHealthChecks(configuration);
            services.AddRedisHealthChecks(configuration);
            return services;
        }

        /// <summary>
        /// Maps health check endpoints
        /// </summary>
        public static IApplicationBuilder MapHealthCheckEndpoints(this IApplicationBuilder app, string prefix = "")
        {
            var path = string.IsNullOrEmpty(prefix) ? "" : "/" + prefix.Trim('/');

            app.UseEndpoints(endpoints =>
            {
                // Health check UI endpoint
                endpoints.MapHealthChecks($"{path}/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                // Liveness probe - just checks if app is running
                endpoints.MapHealthChecks($"{path}/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("live"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                // Readiness probe - checks if app is ready to accept requests
                endpoints.MapHealthChecks($"{path}/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ready"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

            return app;
        }
    }
}
