using System.Collections.Generic;
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

        [Authorize(Roles = "Assistant")]
        [HttpGet("me/invites")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AssistantInviteDto>>>> GetMyInvites()
        {
            try 
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var result = await _assistantService.GetMyInvitesAsync(userId);
                return Ok(ApiResponse<IEnumerable<AssistantInviteDto>>.Success(result, "Lấy danh sách lời mời thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, statusCode = 500, message = ex.Message + " | " + ex.InnerException?.Message });
            }
        }
    }
}
