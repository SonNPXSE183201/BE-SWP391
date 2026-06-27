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

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<PendingBoardVotesResponseDto>>> GetPendingSeries()
        {
            var result = await _boardVotingService.GetPendingVotesPayloadAsync();
            return Ok(ApiResponse<PendingBoardVotesResponseDto>.Success(
                result,
                "Lấy danh sách bộ truyện chờ thẩm định thành công."));
        }

        [HttpGet("rules")]
        public async Task<ActionResult<ApiResponse<BoardVotingRulesDto>>> GetRules()
        {
            var result = await _boardVotingService.BuildRulesDtoAsync();
            return Ok(ApiResponse<BoardVotingRulesDto>.Success(result, "Lấy quy tắc biểu quyết thành công."));
        }
    }
}
