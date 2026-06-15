using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Dashboard
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = "System Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public DashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet("admin")]
        public async Task<ActionResult<ApiResponse<AdminDashboardResponseDto>>> GetAdminDashboard()
        {
            var result = await _adminDashboardService.GetAdminDashboardAsync();
            return Ok(ApiResponse<AdminDashboardResponseDto>.Success(result, "Lấy dữ liệu dashboard quản trị thành công."));
        }
    }
}
