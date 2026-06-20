using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Wallet;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Wallet
{
    [ApiController]
    [Route("api/disputes")]
    [Authorize]
    public class DisputesController : ControllerBase
    {
        private readonly IWalletService _walletService;

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        public DisputesController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DisputeListItemDto>>>> GetDisputes([FromQuery] string? status)
        {
            var disputes = await _walletService.GetDisputesAsync(status);
            return Ok(ApiResponse<IEnumerable<DisputeListItemDto>>.Success(disputes, "Lấy danh sách tranh chấp thành công."));
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<ApiResponse<DisputeDetailDto>>> GetDisputeDetail(int taskId)
        {
            var detail = await _walletService.GetDisputeDetailAsync(taskId);
            return Ok(ApiResponse<DisputeDetailDto>.Success(detail, "Lấy chi tiết tranh chấp thành công."));
        }

        [HttpPost("{taskId}/resolve")]
        public async Task<ActionResult<ApiResponse<object>>> ResolveDispute(int taskId, [FromBody] ResolveDisputeDto dto)
        {
            await _walletService.ResolveDisputeAsync(taskId, dto.AssistantRate, CurrentUserId);
            return Ok(ApiResponse<object>.Success(null, "Giải quyết tranh chấp và phân chia ví thù lao vẽ tranh thành công."));
        }
    }
}
