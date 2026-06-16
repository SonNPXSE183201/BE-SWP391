using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.User;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "System Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> Create([FromBody] CreateUserByAdminDto dto)
        {
            var result = await _userService.CreateUserByAdminAsync(dto);
            return Ok(ApiResponse<UserResponseDto>.Success(result, result.Message ?? "Tạo tài khoản thành công."));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<UserListItemDto>>>> GetUsers(
            [FromQuery] string? role,
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetUsersAsync(role, status, search, page, pageSize);
            return Ok(ApiResponse<PagedResult<UserListItemDto>>.Success(result, "Lấy danh sách người dùng thành công."));
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<List<AssistantResponseDto>>>> GetPendingAssistants()
        {
            var result = await _userService.GetPendingAssistantsAsync();
            return Ok(ApiResponse<List<AssistantResponseDto>>.Success(result, "Lấy danh sách trợ lý chờ duyệt thành công."));
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> Approve(int id)
        {
            var result = await _userService.ApproveUserAsync(id);
            return Ok(ApiResponse<UserResponseDto>.Success(result, result.Message ?? "Phê duyệt tài khoản thành công."));
        }

        [HttpPut("{id}/approve")]
        public async Task<ActionResult<ApiResponse<AssistantProfileResponseDto>>> ApprovePut(
            int id,
            [FromBody] ApproveAssistantRequestDto dto)
        {
            var result = await _userService.ApproveAssistantAsync(id, dto);
            var message = dto.Approved
                ? "Phê duyệt tài khoản trợ lý thành công."
                : "Từ chối phê duyệt tài khoản trợ lý thành công.";
            return Ok(ApiResponse<AssistantProfileResponseDto>.Success(result, message));
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> Reject(int id)
        {
            var result = await _userService.RejectUserAsync(id);
            return Ok(ApiResponse<UserResponseDto>.Success(result, result.Message ?? "Từ chối phê duyệt tài khoản thành công."));
        }

        [HttpPost("{id}/lock")]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> Lock(int id)
        {
            var result = await _userService.LockUserAsync(id);
            return Ok(ApiResponse<UserResponseDto>.Success(result, result.Message ?? "Khóa tài khoản thành công."));
        }
    }
}
