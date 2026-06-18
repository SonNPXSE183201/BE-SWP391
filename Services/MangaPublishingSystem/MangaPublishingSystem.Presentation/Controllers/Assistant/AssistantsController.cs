using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Assistant;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Assistant
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssistantsController : ControllerBase
    {
        private readonly IAssistantService _assistantService;

        public AssistantsController(IAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveAssistants([FromQuery] AssistantFilterDto filter)
        {
            var result = await _assistantService.GetActiveAssistantsAsync(filter);
            return Ok(ApiResponse<PagedResult<AssistantResponseDto>>.Success(result, "Lấy danh sách Assistant thành công."));
        }
    }
}
