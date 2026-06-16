using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Dashboard
{
    public class DashboardStatsResponseDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        [JsonPropertyName("users")]
        public int? Users { get; set; }

        [JsonPropertyName("pendingApprovals")]
        public int? PendingApprovals { get; set; }

        [JsonPropertyName("series")]
        public int? Series { get; set; }

        [JsonPropertyName("transactions")]
        public int? Transactions { get; set; }

        [JsonPropertyName("pendingSeries")]
        public int? PendingSeries { get; set; }

        [JsonPropertyName("approvedSeries")]
        public int? ApprovedSeries { get; set; }

        [JsonPropertyName("inProductionSeries")]
        public int? InProductionSeries { get; set; }

        [JsonPropertyName("assignedSeries")]
        public int? AssignedSeries { get; set; }

        [JsonPropertyName("seriesAwaitingReview")]
        public int? SeriesAwaitingReview { get; set; }

        [JsonPropertyName("mySeries")]
        public int? MySeries { get; set; }

        [JsonPropertyName("openTasks")]
        public int? OpenTasks { get; set; }

        [JsonPropertyName("setupFundBalance")]
        public decimal? SetupFundBalance { get; set; }

        [JsonPropertyName("withdrawableBalance")]
        public decimal? WithdrawableBalance { get; set; }
    }
}
