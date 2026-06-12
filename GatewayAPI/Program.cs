using BuildingBlocks.Security;
using BuildingBlocks.Web;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Setup loading Ocelot configuration file based on environment.
var ocelotFile = $"ocelot.{builder.Environment.EnvironmentName}.json";
var ocelotPath = Path.Combine(builder.Environment.ContentRootPath, ocelotFile);

if (!File.Exists(ocelotPath))
{
    // Fallback to default ocelot.json if env-specific file is missing
    ocelotFile = "ocelot.json";
}

builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

// --- SERVICES ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithBearer();
builder.Services.AddJwtAuthenticationFromEnv(builder.Configuration);
builder.Configuration.AddUserSecrets<Program>();

// Shared configuration from BuildingBlocks
builder.Services.AddCorsFromConfig(builder.Configuration);
builder.Services.AddHealthChecks();

// Integrate Ocelot Gateway Routing Setup
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// --- PIPELINE ---
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Default");

// Setup friendly status code responses
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        response.ContentType = "application/json";
        await response.WriteAsJsonAsync(new
        {
            success = false,
            statusCode = StatusCodes.Status401Unauthorized,
            message = "Unauthorized. Please provide a valid Bearer token."
        });
        return;
    }

    if (response.StatusCode == StatusCodes.Status404NotFound)
    {
        response.ContentType = "application/json";
        await response.WriteAsJsonAsync(new
        {
            success = false,
            statusCode = StatusCodes.Status404NotFound,
            message = "Route not found on API Gateway. Please verify the upstream path and HTTP method."
        });
    }
});

app.UseAuthentication();
app.UseAuthorization();

// Setup Ocelot Route pipeline
 app.UseOcelot().Wait();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
