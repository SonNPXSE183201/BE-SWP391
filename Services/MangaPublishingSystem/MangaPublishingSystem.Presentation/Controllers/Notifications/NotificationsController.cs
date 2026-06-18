using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Notifications
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetMyNotifications()
        {
            var result = await _notificationService.GetNotificationsByUserIdAsync(CurrentUserId);
            return Ok(ApiResponse<IEnumerable<NotificationDto>>.Success(result, "Lấy danh sách thông báo thành công."));
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            var result = await _notificationService.GetUnreadCountAsync(CurrentUserId);
            return Ok(ApiResponse<int>.Success(result, "Lấy số lượng thông báo chưa đọc thành công."));
        }

        [HttpPatch("{id}/read")]
        public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id, CurrentUserId);
            return Ok(ApiResponse<object>.Success(null, "Đánh dấu đã đọc thành công."));
        }

        [HttpPost("mark-all-read")]
        public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(CurrentUserId);
            return Ok(ApiResponse<object>.Success(null, "Đánh dấu tất cả thông báo là đã đọc thành công."));
        }
    }
}
