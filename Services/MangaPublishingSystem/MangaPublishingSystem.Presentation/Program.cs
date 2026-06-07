using BuildingBlocks.Security;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using MangaPublishingSystem.Application;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Presentation.Extensions;
using MangaPublishingSystem.Presentation.Hubs;
using MangaPublishingSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<IUnitOfWork>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearer();
builder.Services.AddJwtAuthenticationFromEnv(config);
builder.Configuration.AddUserSecrets<Program>();

// Database & Infrastructure registrations
builder.Services.AddInfrastructureServices(config);

// Repositories, Services, Unit Of Work registrations
builder.Services.AddApplicationServices();

// Swagger & Auth configuration
builder.Services.AddSwaggerAndAuth(config);

// Presentation configurations
builder.Services.AddPresentationServices();
builder.Services.AddSignalR();

builder.Services.AddCorsFromConfig(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Default");
app.UseGlobalExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHealthChecks("/health");

app.Run();
