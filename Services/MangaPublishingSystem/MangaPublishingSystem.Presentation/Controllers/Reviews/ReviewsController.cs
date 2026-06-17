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

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        public ReviewsController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        [HttpGet("chapters")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Chapter>>>> GetPendingChapters([FromQuery] string? status)
        {
            var chapters = await _chapterService.GetPendingReviewChaptersAsync();
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
    }
}
