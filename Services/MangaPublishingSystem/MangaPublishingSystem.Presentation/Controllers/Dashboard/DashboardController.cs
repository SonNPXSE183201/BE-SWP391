using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.DTOs.Dashboard;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Dashboard
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IDashboardStatsService _dashboardStatsService;

        public DashboardController(
            IAdminDashboardService adminDashboardService,
            IDashboardStatsService dashboardStatsService)
        {
            _adminDashboardService = adminDashboardService;
            _dashboardStatsService = dashboardStatsService;
        }

        private int CurrentUserId
        {
            get
            {
                var rawUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("nameid")?.Value
                    ?? User.FindFirst("sub")?.Value
                    ?? "0";

                return int.TryParse(rawUserId, out var userId) ? userId : 0;
            }
        }

        private string CurrentUserRole =>
            User.FindFirst(ClaimTypes.Role)?.Value
            ?? User.FindFirst("role")?.Value
            ?? User.FindFirst("Role")?.Value
            ?? string.Empty;

        [HttpGet("admin")]
        [Authorize(Roles = "System Admin")]
        public async Task<ActionResult<ApiResponse<AdminDashboardResponseDto>>> GetAdminDashboard()
        {
            var result = await _adminDashboardService.GetAdminDashboardAsync();
            return Ok(ApiResponse<AdminDashboardResponseDto>.Success(result, "Lấy dữ liệu dashboard quản trị thành công."));
        }

        [HttpGet("stats")]
        [Authorize(Roles = "System Admin,Editorial Board,Tantou Editor,Mangaka")]
        public async Task<ActionResult<ApiResponse<DashboardStatsResponseDto>>> GetStats()
        {
            var result = await _dashboardStatsService.GetStatsAsync(CurrentUserId, CurrentUserRole);
            return Ok(ApiResponse<DashboardStatsResponseDto>.Success(result, "Lấy thống kê dashboard thành công."));
        }
    }
}
