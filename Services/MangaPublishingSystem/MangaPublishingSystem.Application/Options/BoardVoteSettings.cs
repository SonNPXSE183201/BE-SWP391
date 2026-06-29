namespace MangaPublishingSystem.Application.Options
{
    /// <summary>
    /// Cấu hình thẩm định Hội đồng (F4.2): Auto/Manual ngưỡng, số TV lẻ, hết hạn vote.
    /// </summary>
    public class BoardVoteSettings
    {
        /// <summary>Auto = ngưỡng ⌊N/2⌋+1 theo số TV Active; Manual = dùng ManualApproveThreshold / ManualRejectThreshold.</summary>
        public string Mode { get; set; } = "Auto";

        /// <summary>Bắt buộc số thành viên Hội đồng Active phải lẻ (7, 5, …) — tránh hòa 50-50.</summary>
        public bool RequireOddActiveBoardMemberCount { get; set; } = true;

        /// <summary>Chế độ Manual: số phiếu Approve tối thiểu (vd. 4 khi N=7).</summary>
        public int? ManualApproveThreshold { get; set; }

        /// <summary>Chế độ Manual: số phiếu Reject tối thiểu.</summary>
        public int? ManualRejectThreshold { get; set; }

        /// <summary>0 = không giới hạn thời gian vote.</summary>
        public int VoteDeadlineHours { get; set; } = 72;

        /// <summary>Hết hạn mà chưa đủ ngưỡng: ResolveByCastMajority (bên nhiều phiếu hơn thắng) | Reject | EscalateToEditor.</summary>
        public string OnDeadline { get; set; } = "ResolveByCastMajority";
    }
}
