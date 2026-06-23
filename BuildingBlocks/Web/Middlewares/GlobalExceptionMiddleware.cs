using BuildingBlocks.Exceptions;
using BuildingBlocks.Web.Errors;
using BuildingBlocks.Web.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.Web.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var descriptor = ErrorCatalog.FromException(exception);
            var errors = (exception as CustomException)?.Errors;

            var innerMsg = exception.InnerException?.Message ?? "No inner exception";
            var message = descriptor.StatusCode == 500 ? $"{descriptor.Message} | DEBUG: {exception.Message} | INNER: {innerMsg}" : descriptor.Message;
            var response = ApiResponse<object>.Failure(descriptor.StatusCode, message, errors);

            var statusCode = descriptor.StatusCode;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}
