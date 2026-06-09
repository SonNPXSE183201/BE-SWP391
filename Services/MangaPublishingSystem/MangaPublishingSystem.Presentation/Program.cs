using BuildingBlocks.Security;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Presentation.Extensions;
using MangaPublishingSystem.Presentation.Hubs;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Controllers
builder.Services.AddControllers();

// FluentValidation
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

// Application DI (QUAN TRỌNG)
builder.Services.AddApplicationServices(config);

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