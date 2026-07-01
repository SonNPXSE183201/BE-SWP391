using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Pages;
using MangaPublishingSystem.Application.DTOs.Regions;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Pages
{
    [ApiController]
    [Route("api/pages")]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _pageService;
        private readonly IRegionService _regionService;
        private readonly IChapterService _chapterService;

        public PagesController(
            IPageService pageService,
            IRegionService regionService,
            IChapterService chapterService)
        {
            _pageService = pageService;
            _regionService = regionService;
            _chapterService = chapterService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("{id}/layers")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LayerDto>>>> GetPageLayers(int id)
        {
            var result = await _pageService.GetPageLayersAsync(id);
            return Ok(ApiResponse<IEnumerable<LayerDto>>.Success(result, "Lấy danh sách các lớp vẽ đè thành công."));
        }

        [Authorize(Roles = "Mangaka,Assistant,Tantou Editor")]
        [HttpGet("{pageId}/regions")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RegionDto>>>> GetPageRegions(int pageId)
        {
            var regions = await _regionService.GetRegionsByPageIdAsync(pageId);
            var result = regions.Select(r => new RegionDto
            {
                Id = r.Id,
                PageId = r.PageId,
                Name = r.Name,
                CoordinatesJson = r.CoordinatesJson,
                CreateAt = r.CreateAt,
                UpdateAt = r.UpdateAt
            });
            return Ok(ApiResponse<IEnumerable<RegionDto>>.Success(result, "Lấy danh sách các vùng khoanh vẽ thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/mark-ready")]
        public async Task<ActionResult<ApiResponse<PageDto>>> MarkPageAsReady([FromRoute] int id)
        {
            var page = await _chapterService.MarkPageAsReadyAsync(id, CurrentUserId);
            var result = new PageDto
            {
                Id = page.Id,
                ChapterId = page.ChapterId,
                PageNumber = page.PageNumber,
                RawImageUrl = page.RawImageUrl,
                CompositeImageUrl = page.CompositeImageUrl,
                BaseLayerUrl = page.BaseLayerUrl,
                Status = page.Status,
                IsApproved = page.IsApproved,
                CreateAt = page.CreateAt,
                UpdateAt = page.UpdateAt
            };
            return Ok(ApiResponse<PageDto>.Success(result, "Đã đánh dấu trang sẵn sàng — không cần sản xuất thêm."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/replace-image")]
        public async Task<ActionResult<ApiResponse<PageDto>>> ReplacePageImage(
            [FromRoute] int id,
            [FromForm] ReplacePageImageDto dto)
        {
            var page = await _chapterService.ReplacePageImageAsync(id, CurrentUserId, dto.File);
            var result = new PageDto
            {
                Id = page.Id,
                ChapterId = page.ChapterId,
                PageNumber = page.PageNumber,
                RawImageUrl = page.RawImageUrl,
                CompositeImageUrl = page.CompositeImageUrl,
                BaseLayerUrl = page.BaseLayerUrl,
                Status = page.Status,
                IsApproved = page.IsApproved,
                CreateAt = page.CreateAt,
                UpdateAt = page.UpdateAt
            };
            return Ok(ApiResponse<PageDto>.Success(result, "Đã tải lại ảnh trang thành công."));
        }
    }
}
