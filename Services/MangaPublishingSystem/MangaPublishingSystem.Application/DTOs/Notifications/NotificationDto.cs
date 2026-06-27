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
            "Wallet_Withdrawal_Pending" => "Yêu cầu rút tiền đã gửi",
            "Wallet_Withdrawal_Admin_Pending" => "Yêu cầu rút tiền mới",
            "Wallet_Deposit_Success" => "Nạp tiền thành công",
            "Series_Submitted" => "Hồ sơ truyện mới được nộp",
            "Series_Submitted_To_Board" => "Series chờ Hội đồng",
            "Series_Approved" => "Hồ sơ truyện được phê duyệt",
            "Series_Rejected" => "Hồ sơ truyện bị từ chối",
            "Series_Revision_Required" => "Editor yêu cầu chỉnh sửa hồ sơ",
            _ => "Thông báo hệ thống"
        };

        public string? Link
        {
            get
            {
                var seriesIdMatch = System.Text.RegularExpressions.Regex.Match(
                    Content ?? string.Empty,
                    @"^#(\d+)#\s*");
                if (seriesIdMatch.Success)
                {
                    return $"/mangaka/series/{seriesIdMatch.Groups[1].Value}";
                }

                return Type switch
                {
                    var t when t.StartsWith("Task") => "/tasks",
                    "Wallet_Withdrawal_Admin_Pending" => "/admin/withdraw-approval",
                    var t when t.StartsWith("Wallet") => "/mangaka/wallet",
                    "Series_Pending_Review" => "/editor/review",
                    "Series_Submitted_To_Board" => seriesIdMatch.Success
                        ? $"/mangaka/series/{seriesIdMatch.Groups[1].Value}"
                        : "/mangaka/series",
                    var t when t.StartsWith("Series") => "/mangaka/series",
                    _ => null
                };
            }
        }
    }
}
