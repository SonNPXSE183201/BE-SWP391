using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Reviews;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Reviews
{
    [ApiController]
    [Route("api/reviews")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly ISeriesService _seriesService;

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        public ReviewsController(IChapterService chapterService, ISeriesService seriesService)
        {
            _chapterService = chapterService;
            _seriesService = seriesService;
        }

        [Authorize(Roles = "Tantou Editor")]
        [HttpGet("chapters")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Chapter>>>> GetPendingChapters([FromQuery] string? status)
        {
            var chapters = await _chapterService.GetPendingReviewChaptersForEditorAsync(CurrentUserId);
            return Ok(ApiResponse<IEnumerable<Chapter>>.Success(chapters, "Lấy danh sách chapter chờ duyệt thành công."));
        }

        [HttpGet("chapters/{chapterId}")]
        public async Task<ActionResult<ApiResponse<Chapter>>> GetChapterById(int chapterId)
        {
            var chapter = await _chapterService.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                return NotFound(ApiResponse<Chapter>.Failure(404, "Không tìm thấy chapter."));
            }
            return Ok(ApiResponse<Chapter>.Success(chapter, "Lấy chi tiết chapter thành công."));
        }

        [HttpPost("chapters/{chapterId}/approve")]
        public async Task<ActionResult<ApiResponse<object>>> ApproveChapter(int chapterId, [FromBody] ApproveChapterDto dto)
        {
            await _chapterService.ApproveChapterAsync(chapterId, CurrentUserId, dto);
            return Ok(ApiResponse<object>.Success(null, "Phê duyệt chapter và giải ngân nhuận bút thành công."));
        }

        [HttpPost("chapters/{chapterId}/revision")]
        public async Task<ActionResult<ApiResponse<object>>> RequestRevision(int chapterId, [FromBody] RejectChapterDto dto)
        {
            await _chapterService.RejectChapterAsync(chapterId, CurrentUserId, dto);
            return Ok(ApiResponse<object>.Success(null, "Yêu cầu chỉnh sửa chapter thành công."));
        }

        [HttpPost("chapters/{chapterId}/reject")]
        public async Task<ActionResult<ApiResponse<object>>> RejectChapter(int chapterId, [FromBody] RejectChapterDto dto)
        {
            await _chapterService.RejectChapterAsync(chapterId, CurrentUserId, dto);
            return Ok(ApiResponse<object>.Success(null, "Từ chối chapter thành công."));
        }

        [Authorize(Roles = "Tantou Editor")]
        [HttpGet("series/pending")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeriesReviewDto>>>> GetPendingSeriesForEditor()
        {
            var result = await _seriesService.GetPendingReviewSeriesForEditorAsync(CurrentUserId);
            return Ok(ApiResponse<IEnumerable<SeriesReviewDto>>.Success(result, "Lấy danh sách series chờ duyệt thành công."));
        }

        [HttpGet("series/{id}")]
        public async Task<ActionResult<ApiResponse<SeriesReviewDto>>> GetSeriesReview(int id)
        {
            var review = await _seriesService.GetSeriesReviewAsync(id);
            return Ok(ApiResponse<SeriesReviewDto>.Success(review, "Lấy thông tin duyệt bộ truyện thành công."));
        }

        [Authorize(Roles = "Tantou Editor")]
        [HttpPost("series/{id}/submit-to-board")]
        public async Task<ActionResult<ApiResponse<object>>> SubmitToBoard(int id, [FromBody] SubmitToBoardDto dto)
        {
            await _seriesService.SubmitSeriesToBoardAsync(id, CurrentUserId, dto);
            return Ok(ApiResponse<object>.Success(null, "Gửi bộ truyện lên hội đồng thẩm định thành công."));
        }

        [Authorize(Roles = "Tantou Editor")]
        [HttpPost("series/{id}/require-revision")]
        public async Task<ActionResult<ApiResponse<object>>> RequireSeriesRevision(int id, [FromBody] RequireSeriesRevisionDto dto)
        {
            await _seriesService.RequireSeriesRevisionAsync(id, CurrentUserId, dto.Comment);
            return Ok(ApiResponse<object>.Success(null, "Yêu cầu chỉnh sửa bộ truyện thành công."));
        }
    }
}

