using System.Collections.Generic;
using System.Linq;
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
    [Route("api/series")]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize(Roles = "Mangaka")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SeriesDto>>> Create([FromBody] CreateSeriesDto createDto)
        {
            int mangakaId = CurrentUserId;
            var series = await _seriesService.CreateSeriesAsync(mangakaId, createDto);
            return Ok(ApiResponse<SeriesDto>.Success(series, "Tạo hồ sơ bộ truyện mới thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/submit-review")]
        public async Task<ActionResult<ApiResponse<object>>> SubmitForReview([FromRoute] int id, [FromBody] SubmitSeriesReviewDto submitDto)
        {
            int mangakaId = CurrentUserId;
            await _seriesService.SubmitForReviewAsync(id, mangakaId, submitDto);
            return Ok(ApiResponse<object>.Success(null, "Gửi hồ sơ bộ truyện duyệt thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("absence")]
        public async Task<ActionResult<ApiResponse<object>>> SetAbsenceStatus([FromQuery] bool onLeave)
        {
            int mangakaId = CurrentUserId;
            await _seriesService.SetAbsenceStatusAsync(mangakaId, onLeave);
            var message = onLeave ? "Bật trạng thái nghỉ phép thành công." : "Tắt trạng thái nghỉ phép thành công.";
            return Ok(ApiResponse<object>.Success(null, message));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpGet("my-list")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeriesDto>>>> GetMySeries()
        {
            int mangakaId = CurrentUserId;
            var seriesList = await _seriesService.GetSeriesByMangakaIdAsync(mangakaId);
            return Ok(ApiResponse<IEnumerable<SeriesDto>>.Success(seriesList, "Lấy danh sách bộ truyện thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPut("{id}/manuscript")]
        public async Task<ActionResult<ApiResponse<object>>> SubmitDraftManuscript([FromRoute] int id, [FromBody] SubmitDraftManuscriptDto dto)
        {
            int mangakaId = CurrentUserId;
            await _seriesService.SubmitDraftManuscriptAsync(id, mangakaId, dto.DraftManuscriptUrl);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật bản thảo nháp của bộ truyện thành công."));
        }

        [Authorize(Roles = "Tantou Editor")]
        [HttpPost("{id}/editor-evaluate")]
        public async Task<ActionResult<ApiResponse<object>>> EvaluateSeries([FromRoute] int id, [FromBody] EditorEvaluationDto dto)
        {
            int editorId = CurrentUserId;
            await _seriesService.EvaluateSeriesByEditorAsync(id, editorId, dto);
            var msg = dto.IsApproved 
                ? "Thẩm định bản thảo nháp thành công. Truyện đã được trình lên Hội đồng duyệt."
                : "Từ chối bản thảo nháp thành công. Truyện đã được trả về trạng thái Draft.";
            return Ok(ApiResponse<object>.Success(null, msg));
        }

        [Authorize(Roles = "Editorial Board")]
        [HttpPost("{id}/votes")]
        public async Task<ActionResult<ApiResponse<object>>> CastVote([FromRoute] int id, [FromBody] BoardVoteDto dto)
        {
            int boardMemberId = CurrentUserId;
            await _seriesService.CastBoardVoteAsync(id, boardMemberId, dto);
            return Ok(ApiResponse<object>.Success(null, "Bỏ phiếu bình chọn bộ truyện thành công."));
        }

        [Authorize(Roles = "Editorial Board")]
        [HttpPost("{id}/board-decision")]
        public async Task<ActionResult<ApiResponse<object>>> FinalizeDecision([FromRoute] int id, [FromBody] BoardDecisionDto dto)
        {
            await _seriesService.FinalizeBoardDecisionAsync(id, dto);
            var msg = dto.IsApproved
                ? "Hội đồng quyết định thông qua bộ truyện thành công."
                : "Hội đồng quyết định từ chối bộ truyện thành công.";
            return Ok(ApiResponse<object>.Success(null, msg));
        }


    }

    public class SubmitDraftManuscriptDto
    {
        public string DraftManuscriptUrl { get; set; } = null!;
    }
}
