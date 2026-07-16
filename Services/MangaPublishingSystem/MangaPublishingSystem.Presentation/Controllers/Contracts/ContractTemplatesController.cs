using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Contracts;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Contracts
{
    [ApiController]
    [Route("api/contract-templates")]
    [Authorize(Roles = "System Admin")]
    public class ContractTemplatesController : ControllerBase
    {
        private readonly IContractTemplateService _templateService;

        public ContractTemplatesController(IContractTemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ContractTemplateDto>>>> GetAll()
        {
            var templates = await _templateService.GetTemplatesAsync();
            return Ok(ApiResponse<IEnumerable<ContractTemplateDto>>.Success(templates));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ContractTemplateDto>>> GetById(int id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            return Ok(ApiResponse<ContractTemplateDto>.Success(template));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContractTemplateDto>>> Create([FromBody] CreateContractTemplateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(ApiResponse<object>.Failure(401, "Không xác định được người dùng."));
            }

            var result = await _templateService.CreateTemplateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ContractTemplateDto>.Created(result, "Tạo mẫu hợp đồng thành công."));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ContractTemplateDto>>> Update(int id, [FromBody] UpdateContractTemplateDto dto)
        {
            var result = await _templateService.UpdateTemplateAsync(id, dto);
            return Ok(ApiResponse<ContractTemplateDto>.Success(result, "Cập nhật mẫu hợp đồng thành công."));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            await _templateService.DeleteTemplateAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Xóa mẫu hợp đồng thành công."));
        }
    }
}
