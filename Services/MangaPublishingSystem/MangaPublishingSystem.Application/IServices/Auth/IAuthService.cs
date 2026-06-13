using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Auth;

namespace MangaPublishingSystem.Application.IServices.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<RegisterResponseDto> RegisterAssistantAsync(RegisterDto registerDto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task ForgotPasswordRequestAsync(ForgotPasswordRequestDto requestDto);
        Task ForgotPasswordResetAsync(ForgotPasswordResetDto resetDto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto tokenDto);
        Task LogoutAsync(LogoutDto logoutDto);
    }
}
