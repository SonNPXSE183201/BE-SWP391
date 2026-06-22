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

        [Authorize(Roles = "Mangaka")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RegionDto>>> UpdateRegion(int id, [FromBody] UpdateRegionDto updateDto)
        {
            var region = await _regionService.UpdateRegionAsync(id, updateDto);
            var result = new RegionDto
            {
                Id = region.Id,
                PageId = region.PageId,
                Name = region.Name,
                CoordinatesJson = region.CoordinatesJson,
                CreateAt = region.CreateAt,
                UpdateAt = region.UpdateAt
            };
            return Ok(ApiResponse<RegionDto>.Success(result, "Cập nhật phân vùng Canvas thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteRegion(int id)
        {
            await _regionService.DeleteRegionAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Xóa phân vùng Canvas thành công."));
        }
    }
}
