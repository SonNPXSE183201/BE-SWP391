using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Series
{
    [ApiController]
    [Route("api/series/{seriesId:int}/team")]
    [Authorize]
    public class SeriesTeamController : ControllerBase
    {
        private readonly ISeriesTeamService _seriesTeamService;

        public SeriesTeamController(ISeriesTeamService seriesTeamService)
        {
            _seriesTeamService = seriesTeamService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private bool IsMangaka => User.IsInRole("Mangaka");

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeriesAssistantDto>>>> GetTeam([FromRoute] int seriesId)
        {
            var result = await _seriesTeamService.GetTeamMembersAsync(seriesId, CurrentUserId, IsMangaka);
            return Ok(ApiResponse<IEnumerable<SeriesAssistantDto>>.Success(result, "Lấy danh sách nhóm thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeriesAssistantDto>>>> GetActiveTeam([FromRoute] int seriesId)
        {
            var result = await _seriesTeamService.GetActiveTeamForAssignmentAsync(seriesId, CurrentUserId);
            return Ok(ApiResponse<IEnumerable<SeriesAssistantDto>>.Success(result, "Lấy danh sách trợ lý có thể giao việc thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("invite")]
        public async Task<ActionResult<ApiResponse<SeriesAssistantDto>>> Invite(
            [FromRoute] int seriesId,
            [FromBody] InviteSeriesAssistantDto dto)
        {
            var result = await _seriesTeamService.InviteAssistantAsync(seriesId, CurrentUserId, dto);
            return Ok(ApiResponse<SeriesAssistantDto>.Success(result, "Đã gửi lời mời tham gia nhóm dự án."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpPost("respond")]
        public async Task<ActionResult<ApiResponse<SeriesAssistantDto>>> Respond(
            [FromRoute] int seriesId,
            [FromBody] RespondSeriesInviteDto dto)
        {
            var result = await _seriesTeamService.RespondToInviteAsync(seriesId, CurrentUserId, dto);
            var message = dto.Accept ? "Đã chấp nhận lời mời tham gia dự án." : "Đã từ chối lời mời.";
            return Ok(ApiResponse<SeriesAssistantDto>.Success(result, message));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpDelete("{assistantId:int}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoveMember(
            [FromRoute] int seriesId,
            [FromRoute] int assistantId,
            [FromQuery] string? roleToRemove = null)
        {
            await _seriesTeamService.RemoveMemberAsync(seriesId, CurrentUserId, assistantId, roleToRemove);
            var message = string.IsNullOrWhiteSpace(roleToRemove) ? "Đã gỡ thành viên khỏi nhóm." : $"Đã gỡ vai trò {roleToRemove} của thành viên.";
            return Ok(ApiResponse<object>.Success(null, message));
        }
    }
}
