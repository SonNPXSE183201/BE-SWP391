using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Chapters;
using MangaPublishingSystem.Application.DTOs.Pages;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Chapters
{
    [ApiController]
    [Route("api/chapters")]
    public class ChaptersController : ControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly IPageService _pageService;

        public ChaptersController(IChapterService chapterService, IPageService pageService)
        {
            _chapterService = chapterService;
            _pageService = pageService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ChapterDto>>> GetChapterById([FromRoute] int id)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null)
            {
                return NotFound(ApiResponse<ChapterDto>.Failure(404, "Không tìm thấy chương truyện này."));
            }

            var result = new ChapterDto
            {
                Id = chapter.Id,
                SeriesId = chapter.SeriesId,
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title,
                ValidPageCount = chapter.ValidPageCount,
                AppliedGenkouryoPrice = chapter.AppliedGenkouryoPrice,
                SubmissionDeadline = chapter.SubmissionDeadline,
                QcChecklistData = chapter.QcChecklistData,
                Status = chapter.Status,
                CreateAt = chapter.CreateAt,
                UpdateAt = chapter.UpdateAt
            };

            return Ok(ApiResponse<ChapterDto>.Success(result, "Lấy thông tin chi tiết chương truyện thành công."));
        }

        [HttpGet("{id}/pages")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PageDto>>>> GetPagesByChapterId([FromRoute] int id)
        {
            var pages = await _pageService.GetPagesByChapterIdAsync(id);
            var result = pages.Select(p => new PageDto
            {
                Id = p.Id,
                ChapterId = p.ChapterId,
                PageNumber = p.PageNumber,
                RawImageUrl = p.RawImageUrl,
                CompositeImageUrl = p.CompositeImageUrl,
                BaseLayerUrl = p.BaseLayerUrl,
                Status = p.Status,
                IsApproved = p.IsApproved,
                CreateAt = p.CreateAt,
                UpdateAt = p.UpdateAt
            }).ToList();

            return Ok(ApiResponse<IEnumerable<PageDto>>.Success(result, "Lấy danh sách trang truyện thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/pages")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PageDto>>>> AddPages([FromRoute] int id, [FromForm] AddChapterPagesDto dto)
        {
            var pages = await _chapterService.AddPagesAsync(id, CurrentUserId, dto.Pages);
            var result = pages.Select(p => new PageDto
            {
                Id = p.Id,
                ChapterId = p.ChapterId,
                PageNumber = p.PageNumber,
                RawImageUrl = p.RawImageUrl,
                CompositeImageUrl = p.CompositeImageUrl,
                BaseLayerUrl = p.BaseLayerUrl,
                Status = p.Status,
                IsApproved = p.IsApproved,
                CreateAt = p.CreateAt,
                UpdateAt = p.UpdateAt
            }).ToList();

            return Ok(ApiResponse<IEnumerable<PageDto>>.Success(result, "Tải lên trang truyện bổ sung thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpGet("{id}/production-readiness")]
        public async Task<ActionResult<ApiResponse<ChapterProductionReadinessDto>>> GetProductionReadiness([FromRoute] int id)
        {
            var readiness = await _chapterService.GetProductionReadinessAsync(id, CurrentUserId);
            return Ok(ApiResponse<ChapterProductionReadinessDto>.Success(readiness, "Lấy trạng thái sản xuất chapter thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/submit-for-review")]
        public async Task<ActionResult<ApiResponse<ChapterDto>>> SubmitForReview([FromRoute] int id)
        {
            var chapter = await _chapterService.SubmitChapterForReviewAsync(id, CurrentUserId);
            var result = new ChapterDto
            {
                Id = chapter.Id,
                SeriesId = chapter.SeriesId,
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title,
                ValidPageCount = chapter.ValidPageCount,
                AppliedGenkouryoPrice = chapter.AppliedGenkouryoPrice,
                Status = chapter.Status,
                CreateAt = chapter.CreateAt,
                UpdateAt = chapter.UpdateAt
            };
            return Ok(ApiResponse<ChapterDto>.Success(result, "Đã nộp chapter lên Editor thành công."));
        }
    }
}
