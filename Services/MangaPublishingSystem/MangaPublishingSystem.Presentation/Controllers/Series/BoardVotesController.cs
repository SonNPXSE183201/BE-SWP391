using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Series
{
    [ApiController]
    [Route("api/votes")]
    [Authorize(Roles = "Editorial Board")]
    public class BoardVotesController : ControllerBase
    {
        private readonly IBoardVotingService _boardVotingService;

        public BoardVotesController(IBoardVotingService boardVotingService)
        {
            _boardVotingService = boardVotingService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<PendingBoardVotesResponseDto>>> GetPendingSeries()
        {
            var result = await _boardVotingService.GetPendingVotesPayloadAsync(CurrentUserId);
            return Ok(ApiResponse<PendingBoardVotesResponseDto>.Success(
                result,
                "Lấy danh sách biểu quyết thành công."));
        }

        [HttpGet("rules")]
        public async Task<ActionResult<ApiResponse<BoardVotingRulesDto>>> GetRules()
        {
            var result = await _boardVotingService.BuildRulesDtoAsync();
            return Ok(ApiResponse<BoardVotingRulesDto>.Success(result, "Lấy quy tắc biểu quyết thành công."));
        }
    }
}
