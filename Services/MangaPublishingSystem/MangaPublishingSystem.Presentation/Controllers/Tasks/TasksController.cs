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
            var result = MapToTasksDto(task);
            return Ok(ApiResponse<TasksDto>.Success(result, "Tạo nhiệm vụ vẽ và ký quỹ thù lao thành công."));
        }

        [Authorize(Roles = "Mangaka,Assistant")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TasksDto>>> GetTaskById([FromRoute] int id)
        {
            var result = await _tasksService.GetTaskDetailsByIdAsync(id);
            return Ok(ApiResponse<TasksDto>.Success(result, "Lấy thông tin chi tiết nhiệm vụ thành công."));
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
            var result = tasks.Select(MapToTasksDto).ToList();
            return Ok(ApiResponse<IEnumerable<TasksDto>>.Success(result, "Lấy danh sách nhiệm vụ thành công."));
        }

        [Authorize(Roles = "Mangaka,Assistant")]
        [HttpGet("{id}/versions")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TaskVersionDto>>>> GetVersions([FromRoute] int id)
        {
            var versions = await _tasksService.GetTaskVersionsAsync(id);
            var result = versions.Select(MapToTaskVersionDto).ToList();
            return Ok(ApiResponse<IEnumerable<TaskVersionDto>>.Success(result, "Lấy lịch sử các phiên bản vẽ nộp thành công."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpGet("available")]
        public async Task<ActionResult<ApiResponse<PagedResult<TasksDto>>>> GetAvailableTasks([FromQuery] GetAvailableTasksRequest request)
        {
            var result = await _tasksService.GetAvailableTasksAsync(request);
            return Ok(ApiResponse<PagedResult<TasksDto>>.Success(result, "Lấy danh sách nhiệm vụ khả dụng thành công."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpGet("my-tasks")]
        public async Task<ActionResult<ApiResponse<PagedResult<TasksDto>>>> GetMyAssistantTasks([FromQuery] GetAssistantTasksRequest request)
        {
            int assistantId = CurrentUserId;
            var result = await _tasksService.GetAssistantTasksAsync(assistantId, request);
            return Ok(ApiResponse<PagedResult<TasksDto>>.Success(result, "Lấy danh sách nhiệm vụ của bạn thành công."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpPost("{id}/accept")]
        public async Task<ActionResult<ApiResponse<object>>> AcceptTask([FromRoute] int id)
        {
            int assistantId = CurrentUserId;
            await _tasksService.AcceptTaskAsync(id, assistantId);
            return Ok(ApiResponse<object>.Success(null, "Nhận nhiệm vụ thành công."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpPost("{id}/submit")]
        public async Task<ActionResult<ApiResponse<object>>> SubmitTask([FromRoute] int id, [FromBody] SubmitTaskDto submitDto)
        {
            int assistantId = CurrentUserId;
            await _tasksService.SubmitTaskAsync(id, assistantId, submitDto);
            return Ok(ApiResponse<object>.Success(null, "Nộp bài vẽ nhiệm vụ thành công. Đang chờ tác giả duyệt."));
        }

        [Authorize(Roles = "Assistant")]
        [HttpPost("{id}/request-extension")]
        public async Task<ActionResult<ApiResponse<object>>> RequestExtension([FromRoute] int id, [FromBody] RequestExtensionDto extensionDto)
        {
            int assistantId = CurrentUserId;
            await _tasksService.RequestExtensionAsync(id, assistantId, extensionDto);
            return Ok(ApiResponse<object>.Success(null, "Gửi yêu cầu xin gia hạn thành công. Đang chờ tác giả duyệt."));
        }

        [HttpGet("pages/{pageId}/composite")]
        public async Task<IActionResult> GetCompositePage([FromRoute] int pageId)
        {
            var imageBytes = await _tasksService.GetCompositedPageAsync(pageId);
            return File(imageBytes, "image/png");
        }

        private static TasksDto MapToTasksDto(MangaPublishingSystem.Domain.Entities.Tasks t)
        {
            return new TasksDto
            {
                Id = t.Id,
                MangakaId = t.MangakaId,
                RegionId = t.RegionId,
                AssistantId = t.AssistantId,
                Description = t.Description,
                PaymentAmount = t.PaymentAmount,
                Deadline = t.Deadline,
                ExtensionRequestDays = t.ExtensionRequestDays,
                ExtensionReason = t.ExtensionReason,
                ExtensionStatus = t.ExtensionStatus,
                ZIndex_Order = t.ZIndex_Order,
                Status = t.Status,
                Rating = t.Rating,
                FeedbackComment = t.FeedbackComment,
                MangakaName = t.Mangaka?.FullName,
                AssistantName = t.Assistant?.FullName,
                PageNumber = t.Region?.PageId ?? 0,
                PageImageUrl = t.Region?.Page?.RawImageUrl,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            };
        }

        private static TaskVersionDto MapToTaskVersionDto(TaskVersion v)
        {
            return new TaskVersionDto
            {
                Id = v.Id,
                TaskId = v.TaskId,
                VersionNumber = v.VersionNumber,
                SubmittedFileUrl = v.SubmittedFileUrl,
                Status = v.Status,
                SubmittedAt = v.SubmittedAt
            };
        }
    }
}
