using System;
using System.Collections.Generic;
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
    [Route("api/assistant/portfolio")]
    public class AssistantPortfolioController : ControllerBase
    {
        private readonly IAssistantService _assistantService;

        public AssistantPortfolioController(IAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize(Roles = "Mangaka,Assistant")]
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<AssistantPortfolioStatsDto>>> GetStats([FromQuery] int? assistantId)
        {
            int targetId = assistantId ?? CurrentUserId;
            var stats = await _assistantService.GetPortfolioStatsAsync(targetId);
            return Ok(ApiResponse<AssistantPortfolioStatsDto>.Success(stats, "Lấy thống kê portfolio thành công."));
        }

        [Authorize(Roles = "Mangaka,Assistant")]
        [HttpGet("samples")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PortfolioSampleDto>>>> GetSamples([FromQuery] int? assistantId)
        {
            int targetId = assistantId ?? CurrentUserId;
            var samples = await _assistantService.GetPortfolioSamplesAsync(targetId);
            return Ok(ApiResponse<IEnumerable<PortfolioSampleDto>>.Success(samples, "Lấy danh sách mẫu vẽ thành công."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpPost("samples")]
        public async Task<ActionResult<ApiResponse<PortfolioSampleDto>>> CreateSample([FromBody] CreatePortfolioSampleDto dto)
        {
            var sample = await _assistantService.CreatePortfolioSampleAsync(CurrentUserId, dto);
            return Ok(ApiResponse<PortfolioSampleDto>.Success(sample, "Tạo mẫu vẽ thành công."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpPut("samples/{id}")]
        public async Task<ActionResult<ApiResponse<PortfolioSampleDto>>> UpdateSample(int id, [FromBody] UpdatePortfolioSampleDto dto)
        {
            var sample = await _assistantService.UpdatePortfolioSampleAsync(CurrentUserId, id, dto);
            return Ok(ApiResponse<PortfolioSampleDto>.Success(sample, "Cập nhật mẫu vẽ thành công."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpDelete("samples/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteSample(int id)
        {
            await _assistantService.DeletePortfolioSampleAsync(CurrentUserId, id);
            return Ok(ApiResponse<object>.Success(null, "Xóa mẫu vẽ thành công."));
        }
    }
}
