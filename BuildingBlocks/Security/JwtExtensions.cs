using System.Text;
using System.Text.Json;
using BuildingBlocks.Web.Errors;
using BuildingBlocks.Web.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Security
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthenticationFromEnv(this IServiceCollection services, IConfiguration configuration)
        {
            var authority = configuration["Jwt:Authority"] ?? configuration["Jwt__Authority"];
            var audience = configuration["Jwt:Audience"] ?? configuration["Jwt__Audience"];
            var issuer = configuration["Jwt:Issuer"] ?? configuration["Jwt__Issuer"];
            var key = configuration["Jwt:Key"] ?? configuration["Jwt__Key"];
            var requireHttps = bool.TryParse(configuration["Jwt:RequireHttpsMetadata"] ?? configuration["Jwt__RequireHttpsMetadata"], out var https) && https;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = requireHttps;
                    options.SaveToken = true;

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();

                            var response = context.Response;
                            response.StatusCode = StatusCodes.Status401Unauthorized;
                            response.ContentType = "application/json";

                            var descriptor = ErrorCatalog.FromStatusCode(StatusCodes.Status401Unauthorized);
                            var payload = ApiResponse<object>.Failure(descriptor.StatusCode, descriptor.Message);

                            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });

                            await response.WriteAsync(json);
                        },
                        OnForbidden = async context =>
                        {
                            var response = context.Response;
                            response.StatusCode = StatusCodes.Status403Forbidden;
                            response.ContentType = "application/json";

                            var descriptor = ErrorCatalog.FromStatusCode(StatusCodes.Status403Forbidden);
                            var payload = ApiResponse<object>.Failure(descriptor.StatusCode, descriptor.Message);

                            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });

                            await response.WriteAsync(json);
                        }
                    };

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                            ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                            ValidIssuer = issuer,
                            ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                            ValidAudience = audience,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    }
                    else
                    {
                        options.Authority = authority;
                        options.Audience = audience;
                    }
                });

            services.AddAuthorization();
            return services;
        }
    }
}
