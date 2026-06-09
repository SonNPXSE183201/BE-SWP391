using BuildingBlocks.Security;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Middlewares;
using BuildingBlocks.Web.Responses;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.AspNetCore;
using MangaPublishingSystem.Application;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Repositories; // THÊM DÒNG NÀY
using MangaPublishingSystem.Presentation.Extensions;
using MangaPublishingSystem.Presentation.Hubs;
using MangaPublishingSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

<<<<<<< HEAD
// Controllers
builder.Services.AddControllers();

// FluentValidation
=======
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            var response = ApiResponse<object>.Failure(
                StatusCodes.Status400BadRequest,
                "Dữ liệu yêu cầu không hợp lệ.",
                errors
            );

            return new BadRequestObjectResult(response);
        };
    });
>>>>>>> origin/dev
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IUnitOfWork>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearer();

// JWT
builder.Services.AddJwtAuthenticationFromEnv(config);

// User Secrets
builder.Configuration.AddUserSecrets<Program>();

// Infrastructure
builder.Services.AddInfrastructureServices(config);

// Application DI
builder.Services.AddApplicationServices(config);

// THÊM 2 DÒNG NÀY
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

// Presentation
builder.Services.AddPresentationServices();

// SignalR
builder.Services.AddSignalR();

// CORS + HealthCheck
builder.Services.AddCorsFromConfig(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline
app.UseCors("Default");
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHealthChecks("/health");

app.Run();