using MangaPublishingSystem.Domain.Common;

namespace MangaPublishingSystem.Domain.Entities
{
    /// <summary>Singleton cấu hình biểu quyết Hội đồng (luôn dùng bản ghi Id = 1).</summary>
    public class BoardVotingConfig : BaseEntity
    {
        /// <summary>Số giờ chờ trước khi hệ thống tự chốt (mặc định 48).</summary>
        public int AutoResolveHours { get; set; } = 48;

        /// <summary>% thành viên HĐ cần phiếu Approve (vd. 66 ≈ 2/3).</summary>
        public int ApprovalThresholdPercent { get; set; } = 66;

        /// <summary>% thành viên HĐ cần phiếu Reject.</summary>
        public int RejectionThresholdPercent { get; set; } = 66;

        /// <summary>Reject | Escalate | ChairDecides — xử lý khi hòa phiếu sau khi tất cả đã vote.</summary>
        public string TiePolicy { get; set; } = "Escalate";

        /// <summary>Xóa phiếu cũ khi Editor trình lại Hội đồng.</summary>
        public bool ClearVotesOnResubmit { get; set; } = true;

        /// <summary>Cảnh báo khi số TV HĐ đang là số chẵn.</summary>
        public bool RequireOddBoardSize { get; set; } = true;

        /// <summary>RoleId được tính là thành viên Hội đồng (mặc định 3).</summary>
        public int BoardRoleId { get; set; } = 3;

        /// <summary>UserId chủ tịch HĐ — dùng khi TiePolicy = ChairDecides.</summary>
        public int? ChairUserId { get; set; }
    }
}
