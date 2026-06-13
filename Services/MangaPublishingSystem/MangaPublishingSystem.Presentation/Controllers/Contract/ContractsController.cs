using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Contract;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Contract
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

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize(Roles = "System Admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContractDto>>> Create([FromBody] CreateContractDto createDto)
        {
            var contract = await _contractService.CreateContractAsync(createDto);
            return Ok(ApiResponse<ContractDto>.Success(contract, "Tạo hợp đồng thành công. Đang chờ tác giả ký kết."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/accept")]
        public async Task<ActionResult<ApiResponse<object>>> Accept([FromRoute] int id)
        {
            int mangakaId = CurrentUserId;
            await _contractService.AcceptContractAsync(id, mangakaId);
            return Ok(ApiResponse<object>.Success(null, "Ký hợp đồng thành công. Quỹ sản xuất đã được giải ngân về ví tài trợ của bạn."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/decline")]
        public async Task<ActionResult<ApiResponse<object>>> Decline([FromRoute] int id, [FromBody] DeclineContractDto declineDto)
        {
            int mangakaId = CurrentUserId;
            await _contractService.DeclineContractAsync(id, mangakaId, declineDto.DeclineReason);
            return Ok(ApiResponse<object>.Success(null, "Từ chối hợp đồng thành công. Bộ truyện đã được trả về trạng thái Draft."));
        }


    }
}
