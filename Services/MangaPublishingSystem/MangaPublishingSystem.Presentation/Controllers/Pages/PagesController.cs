using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Pages;
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

        public PagesController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [HttpGet("{id}/layers")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LayerDto>>>> GetPageLayers(int id)
        {
            var result = await _pageService.GetPageLayersAsync(id);
            return Ok(ApiResponse<IEnumerable<LayerDto>>.Success(result, "Lấy danh sách các lớp vẽ đè thành công."));
        }
    }
}
