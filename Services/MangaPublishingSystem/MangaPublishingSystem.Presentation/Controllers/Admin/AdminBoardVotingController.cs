using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/board-voting")]
    [Authorize(Roles = "System Admin")]
    public class AdminBoardVotingController : ControllerBase
    {
        private readonly IBoardVotingService _boardVotingService;

        public AdminBoardVotingController(IBoardVotingService boardVotingService)
        {
            _boardVotingService = boardVotingService;
        }

        [HttpGet("config")]
        public async Task<ActionResult<ApiResponse<BoardVotingConfigDto>>> GetConfig()
        {
            var result = await _boardVotingService.GetConfigDtoAsync();
            return Ok(ApiResponse<BoardVotingConfigDto>.Success(result, "Lấy cấu hình biểu quyết thành công."));
        }

        [HttpPut("config")]
        public async Task<ActionResult<ApiResponse<BoardVotingConfigDto>>> UpdateConfig(
            [FromBody] UpdateBoardVotingConfigDto dto)
        {
            var result = await _boardVotingService.UpdateConfigAsync(dto);
            return Ok(ApiResponse<BoardVotingConfigDto>.Success(result, "Cập nhật cấu hình biểu quyết thành công."));
        }

        [HttpGet("rules")]
        public async Task<ActionResult<ApiResponse<BoardVotingRulesDto>>> GetRules()
        {
            var result = await _boardVotingService.BuildRulesDtoAsync();
            return Ok(ApiResponse<BoardVotingRulesDto>.Success(result, "Lấy quy tắc biểu quyết thành công."));
        }

        [HttpGet("escalated")]
        public async Task<ActionResult<ApiResponse<System.Collections.Generic.IEnumerable<SeriesDto>>>> GetEscalated()
        {
            var result = await _boardVotingService.GetEscalatedSeriesAsync();
            return Ok(ApiResponse<System.Collections.Generic.IEnumerable<SeriesDto>>.Success(
                result,
                "Lấy danh sách biểu quyết cần xử lý thủ công thành công."));
        }

        [HttpPost("series/{id}/resolve")]
        public async Task<ActionResult<ApiResponse<object>>> ManualResolve(
            int id,
            [FromBody] ManualResolveBoardVoteDto dto)
        {
            var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _boardVotingService.ManualResolveAsync(id, adminId, dto);
            return Ok(ApiResponse<object>.Success(null, "Quyết định thủ công đã được ghi nhận."));
        }
    }
}
