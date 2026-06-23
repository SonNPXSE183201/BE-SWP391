using System.Collections.Generic;
using System.Linq;
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

        public PagesController(IPageService pageService, IRegionService regionService)
        {
            _pageService = pageService;
            _regionService = regionService;
        }

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
    }
}
