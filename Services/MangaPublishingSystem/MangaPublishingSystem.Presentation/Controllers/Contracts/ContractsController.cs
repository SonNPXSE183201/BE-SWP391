using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Contracts;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Contracts
{
    [ApiController]
    [Route("api/contracts")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractsController(IContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpGet]
        [Authorize(Roles = "System Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ContractDto>>>> GetAll()
        {
            var result = await _contractService.GetContractsAsync();
            return Ok(ApiResponse<IEnumerable<ContractDto>>.Success(result, "Lấy danh sách hợp đồng thành công."));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "System Admin,Mangaka")]
        public async Task<ActionResult<ApiResponse<ContractDto>>> GetById(int id)
        {
            var result = await _contractService.GetContractByIdAsync(id);
            return Ok(ApiResponse<ContractDto>.Success(result, "Lấy chi tiết hợp đồng thành công."));
        }

        [HttpPost("generate")]
        [Authorize(Roles = "System Admin")]
        public async Task<ActionResult<ApiResponse<ContractDto>>> Generate([FromBody] CreateContractDto dto)
        {
            var result = await _contractService.GenerateContractAsync(dto);
            return Ok(ApiResponse<ContractDto>.Success(result, "Tạo hợp đồng thành công."));
        }

        [HttpPost("{id}/sign")]
        [Authorize(Roles = "Mangaka")]
        public async Task<ActionResult<ApiResponse<ContractDto>>> Sign(int id)
        {
            var result = await _contractService.SignContractAsync(id);
            return Ok(ApiResponse<ContractDto>.Success(result, "Ký hợp đồng thành công."));
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Mangaka")]
        public async Task<ActionResult<ApiResponse<ContractDto>>> Reject(int id)
        {
            var result = await _contractService.RejectContractAsync(id);
            return Ok(ApiResponse<ContractDto>.Success(result, "Từ chối hợp đồng thành công."));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "System Admin")]
        public async Task<ActionResult<ApiResponse<ContractDto>>> Update(int id, [FromBody] UpdateContractDto dto)
        {
            var result = await _contractService.UpdateContractAsync(id, dto);
            return Ok(ApiResponse<ContractDto>.Success(result, "Cập nhật hợp đồng thành công."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "System Admin")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            await _contractService.DeleteContractAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Xóa hợp đồng thành công."));
        }
    }
}
