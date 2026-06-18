using System;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/reconciliation")]
    [Authorize(Roles = "System Admin")]
    public class AdminReconciliationController : ControllerBase
    {
        private readonly IAdminReconciliationService _adminReconciliationService;

        public AdminReconciliationController(IAdminReconciliationService adminReconciliationService)
        {
            _adminReconciliationService = adminReconciliationService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<ReconciliationResponseDto>>> GetReconciliation(
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] string? status,
            [FromQuery] string? referenceCode)
        {
            DateTime? fromDate = DateTime.TryParse(from, out var parsedFrom) ? parsedFrom.ToUniversalTime() : null;
            DateTime? toDate = DateTime.TryParse(to, out var parsedTo) ? parsedTo.ToUniversalTime() : null;

            var result = await _adminReconciliationService.GetReconciliationAsync(fromDate, toDate, status, referenceCode);
            return Ok(ApiResponse<ReconciliationResponseDto>.Success(result, "Lấy dữ liệu đối soát VNPay thành công."));
        }
    }
}
