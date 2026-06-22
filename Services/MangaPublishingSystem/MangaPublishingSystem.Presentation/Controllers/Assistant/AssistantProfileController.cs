using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Assistant;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Assistant
{
    [ApiController]
    [Route("api/assistant/profile")]
    public class AssistantProfileController : ControllerBase
    {
        private readonly IAssistantService _assistantService;

        public AssistantProfileController(IAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize(Roles = "Assistant")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<AssistantProfileDto>>> GetProfile()
        {
            var profile = await _assistantService.GetProfileAsync(CurrentUserId);
            return Ok(ApiResponse<AssistantProfileDto>.Success(profile, "Lấy thông tin hồ sơ Assistant thành công."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpPut]
        public async Task<ActionResult<ApiResponse<AssistantProfileDto>>> UpdateProfile([FromBody] UpdateAssistantProfileDto dto)
        {
            var profile = await _assistantService.UpdateProfileAsync(CurrentUserId, dto);
            return Ok(ApiResponse<AssistantProfileDto>.Success(profile, "Cập nhật hồ sơ Assistant thành công."));
        }
    }
}
