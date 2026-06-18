using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Regions;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Regions
{
    [ApiController]
    [Route("api/regions")]
    public class RegionsController : ControllerBase
    {
        private readonly IRegionService _regionService;

        public RegionsController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<RegionDto>>> CreateRegion([FromBody] CreateRegionDto createDto)
        {
            var region = await _regionService.CreateRegionAsync(createDto);
            var result = new RegionDto
            {
                Id = region.Id,
                PageId = region.PageId,
                Name = region.Name,
                CoordinatesJson = region.CoordinatesJson,
                CreateAt = region.CreateAt,
                UpdateAt = region.UpdateAt
            };
            return Ok(ApiResponse<RegionDto>.Success(result, "Tạo phân vùng Canvas thành công."));
        }
    }
}
