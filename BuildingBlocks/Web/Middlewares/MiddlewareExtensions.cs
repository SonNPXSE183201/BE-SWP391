using System.Text.Json;
using BuildingBlocks.Web.Errors;
using BuildingBlocks.Web.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<GlobalExceptionMiddleware>();
            builder.UseFriendlyStatusCodeResponses();
            return builder;
        }

        public static IApplicationBuilder UseFriendlyStatusCodeResponses(this IApplicationBuilder builder)
        {
            return builder.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;

                if (response.HasStarted || !ShouldHandle(response.StatusCode))
                {
                    return;
                }

                response.ContentType = "application/json";

                var descriptor = ErrorCatalog.FromStatusCode(response.StatusCode);
                var payload = ApiResponse<object>.Failure(descriptor.StatusCode, descriptor.Message);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await response.WriteAsync(JsonSerializer.Serialize(payload, options));
            });
        }

        private static bool ShouldHandle(int statusCode)
        {
            return statusCode == StatusCodes.Status401Unauthorized
                   || statusCode == StatusCodes.Status403Forbidden
                   || statusCode == StatusCodes.Status404NotFound;
        }
    }
}
