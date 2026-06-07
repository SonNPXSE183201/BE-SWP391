using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Auth;

namespace MangaPublishingSystem.Application.IServices.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
}
