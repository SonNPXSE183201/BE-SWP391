using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/contracts")]
    [Authorize(Roles = "System Admin")]
    public class AdminContractsController : ControllerBase
    {
        private readonly IAdminContractService _adminContractService;

        public AdminContractsController(IAdminContractService adminContractService)
        {
            _adminContractService = adminContractService;
        }

        [HttpGet("series")]
        public async Task<ActionResult<ApiResponse<List<ApprovedSeriesContractDto>>>> GetApprovedSeries()
        {
            var result = await _adminContractService.GetApprovedSeriesAsync();
            return Ok(ApiResponse<List<ApprovedSeriesContractDto>>.Success(result, "Lấy danh sách series chờ lập hợp đồng thành công."));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateContractResponseDto>>> CreateContract([FromBody] CreateContractRequestDto dto)
        {
            var result = await _adminContractService.CreateContractAsync(dto);
            return Ok(ApiResponse<CreateContractResponseDto>.Success(result, "Đã tạo hợp đồng và thiết lập nhuận bút thành công."));
        }

        [HttpPut("{contractId}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateContract(int contractId, [FromBody] UpdateContractRequestDto dto)
        {
            await _adminContractService.UpdateContractAsync(contractId, dto);
            return Ok(ApiResponse<object>.Success(null!, "Đã tạo phụ lục hợp đồng và cập nhật đơn giá nhuận bút thành công."));
        }
    }
}
