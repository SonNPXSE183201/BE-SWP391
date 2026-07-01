using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    /// <summary>Singleton cấu hình biểu quyết Hội đồng (luôn dùng bản ghi Id = 1).</summary>
    public class BoardVotingConfig : BaseEntity
    {
        /// <summary>Số giờ chờ trước khi hệ thống tự chốt (mặc định 48).</summary>
        public int AutoResolveHours { get; set; } = 48;

        /// <summary>% trọng số phiếu cần để Approve (mặc định 51 — đa số đơn giản).</summary>
        public int ApprovalThresholdPercent { get; set; } = 51;

        /// <summary>Xóa phiếu cũ khi Editor trình lại Hội đồng.</summary>
        public bool ClearVotesOnResubmit { get; set; } = true;

        /// <summary>RoleId được tính là thành viên Hội đồng (mặc định 3).</summary>
        public int BoardRoleId { get; set; } = 3;

        /// <summary>UserId Chủ tịch HĐ — được gán trọng số phiếu động.</summary>
        public int? ChairUserId { get; set; }
    }
}
