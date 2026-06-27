using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/platform-wallet")]
    [Authorize(Roles = "System Admin")]
    public class AdminPlatformWalletController : ControllerBase
    {
        private readonly IPlatformWalletService _platformWalletService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminPlatformWalletController(
            IPlatformWalletService platformWalletService,
            IHttpContextAccessor httpContextAccessor)
        {
            _platformWalletService = platformWalletService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PlatformWalletDto>>> GetTreasury()
        {
            var result = await _platformWalletService.GetTreasuryAsync();
            return Ok(ApiResponse<PlatformWalletDto>.Success(result, "Lấy thông tin ví quỹ NXB thành công."));
        }

        [HttpPost("top-up")]
        public async Task<ActionResult<ApiResponse<string>>> TopUp([FromBody] TopUpPlatformWalletDto dto)
        {
            var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var ipAddr = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (ipAddr == "::1") ipAddr = "127.0.0.1";

            var paymentUrl = await _platformWalletService.InitiateTopUpTreasuryAsync(adminId, dto, ipAddr);
            return Ok(ApiResponse<string>.Success(
                paymentUrl,
                "Khởi tạo nạp quỹ thành công. Vui lòng thanh toán qua VNPay Sandbox."));
        }
    }
}
