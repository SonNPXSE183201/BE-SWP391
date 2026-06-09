using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Auth;
using MangaPublishingSystem.Application.IServices.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(ApiResponse<AuthResponseDto>.Success(result, "Đăng nhập thành công."));
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.RegisterAssistantAsync(registerDto);
            return Ok(ApiResponse<RegisterResponseDto>.Success(result, result.Message));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(ApiResponse<object>.Failure(401, "Người dùng chưa xác thực hoặc phiên đăng nhập không hợp lệ."));
            }

            await _authService.ChangePasswordAsync(userId, changePasswordDto);
            return Ok(ApiResponse<object>.Success(null, "Đổi mật khẩu thành công."));
        }

        [HttpPost("forgot-password/request")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPasswordRequest([FromBody] ForgotPasswordRequestDto requestDto)
        {
            await _authService.ForgotPasswordRequestAsync(requestDto);
            return Ok(ApiResponse<object>.Success(null, "Mã OTP xác thực đã được gửi tới email của bạn. Vui lòng kiểm tra hộp thư."));
        }

        [HttpPost("forgot-password/reset")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPasswordReset([FromBody] ForgotPasswordResetDto resetDto)
        {
            await _authService.ForgotPasswordResetAsync(resetDto);
            return Ok(ApiResponse<object>.Success(null, "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập bằng mật khẩu mới."));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenDto);
            return Ok(ApiResponse<AuthResponseDto>.Success(result, "Làm mới token thành công."));
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutDto logoutDto)
        {
            await _authService.LogoutAsync(logoutDto);
            return Ok(ApiResponse<object>.Success(null, "Đăng xuất thành công."));
        }
    }
}
