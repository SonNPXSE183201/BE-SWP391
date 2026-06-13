using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Tasks;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Tasks
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITasksService _tasksService;

        public TasksController(ITasksService tasksService)
        {
            _tasksService = tasksService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize(Roles = "Mangaka")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TasksDto>>> CreateTask([FromBody] CreateTaskDto createDto)
        {
            int mangakaId = CurrentUserId;
            var task = await _tasksService.CreateTaskAsync(mangakaId, createDto);
            return Ok(ApiResponse<TasksDto>.Success(task, "Tạo nhiệm vụ vẽ và ký quỹ thù lao thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/approve")]
        public async Task<ActionResult<ApiResponse<object>>> ApproveTask([FromRoute] int id, [FromBody] ApproveTaskDto approveDto)
        {
            int mangakaId = CurrentUserId;
            await _tasksService.ApproveSubmissionAsync(id, mangakaId, approveDto);
            return Ok(ApiResponse<object>.Success(null, "Phê duyệt nghiệm thu bài nộp thành công. Thù lao đã chuyển về ví Assistant."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/reject")]
        public async Task<ActionResult<ApiResponse<object>>> RejectTask([FromRoute] int id, [FromBody] RejectTaskDto rejectDto)
        {
            int mangakaId = CurrentUserId;
            await _tasksService.RejectSubmissionAsync(id, mangakaId, rejectDto);
            return Ok(ApiResponse<object>.Success(null, "Từ chối bài vẽ thành công. Yêu cầu sửa đổi đã gửi tới Assistant kèm gia hạn."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/extension-approval")]
        public async Task<ActionResult<ApiResponse<object>>> HandleExtension([FromRoute] int id, [FromQuery] bool approve)
        {
            int mangakaId = CurrentUserId;
            await _tasksService.HandleExtensionRequestAsync(id, mangakaId, approve);
            var message = approve ? "Chấp thuận yêu cầu gia hạn thành công." : "Từ chối yêu cầu gia hạn thành công.";
            return Ok(ApiResponse<object>.Success(null, message));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/emergency-cancel")]
        public async Task<ActionResult<ApiResponse<object>>> EmergencyCancel([FromRoute] int id)
        {
            int mangakaId = CurrentUserId;
            await _tasksService.EmergencyCancelTaskAsync(id, mangakaId);
            return Ok(ApiResponse<object>.Success(null, "Hủy nhiệm vụ khẩn cấp thành công. Quỹ ký quỹ đã được hoàn trả lại ví của bạn."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpGet("mangaka-list")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TasksDto>>>> GetMyTasks()
        {
            int mangakaId = CurrentUserId;
            var tasks = await _tasksService.GetTasksByMangakaIdAsync(mangakaId);
            return Ok(ApiResponse<IEnumerable<TasksDto>>.Success(tasks, "Lấy danh sách nhiệm vụ thành công."));
        }

        [Authorize(Roles = "Mangaka,Assistant")]
        [HttpGet("{id}/versions")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TaskVersionDto>>>> GetVersions([FromRoute] int id)
        {
            var versions = await _tasksService.GetTaskVersionsAsync(id);
            return Ok(ApiResponse<IEnumerable<TaskVersionDto>>.Success(versions, "Lấy lịch sử các phiên bản vẽ nộp thành công."));
        }

        [HttpGet("pages/{pageId}/composite")]
        public async Task<IActionResult> GetCompositePage([FromRoute] int pageId)
        {
            var imageBytes = await _tasksService.GetCompositedPageAsync(pageId);
            return File(imageBytes, "image/png");
        }


    }
}
