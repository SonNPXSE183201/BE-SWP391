using System;

namespace MangaPublishingSystem.Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreateAt { get; set; }

        public string Title => Type switch
        {
            "Task_Assigned" => "Nhiệm vụ vẽ mới được giao",
            "Task_Submitted" => "Nhiệm vụ vẽ đã nộp bài",
            "Task_Accepted" => "Nhiệm vụ đã được tiếp nhận",
            "Task_Completed" => "Nhiệm vụ đã hoàn thành",
            "Task_Approved" => "Nhiệm vụ được phê duyệt",
            "Task_Rejected" => "Nhiệm vụ bị từ chối/yêu cầu sửa đổi",
            "Task_Extension_Pending" => "Yêu cầu gia hạn nhiệm vụ mới",
            "Task_Extension_Approved" => "Yêu cầu gia hạn được chấp nhận",
            "Task_Extension_Rejected" => "Yêu cầu gia hạn bị từ chối",
            "Task_Cancelled" => "Nhiệm vụ đã bị hủy",
            "Wallet_Withdrawal_Approve" => "Yêu cầu rút tiền được duyệt",
            "Wallet_Withdrawal_Reject" => "Yêu cầu rút tiền bị từ chối",
            "Series_Submitted" => "Hồ sơ truyện mới được nộp",
            "Series_Approved" => "Hồ sơ truyện được phê duyệt",
            "Series_Rejected" => "Hồ sơ truyện bị từ chối",
            _ => "Thông báo hệ thống"
        };

        public string? Link => Type switch
        {
            var t when t.StartsWith("Task") => "/tasks",
            var t when t.StartsWith("Wallet") => "/wallet",
            var t when t.StartsWith("Series") => "/series",
            _ => null
        };
    }
}
