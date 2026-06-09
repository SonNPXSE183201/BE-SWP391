using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.Common.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
