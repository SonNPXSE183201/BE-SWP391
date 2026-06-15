using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {
        [JsonPropertyName("users")]
        public int Users { get; set; }

        [JsonPropertyName("approvals")]
        public int Approvals { get; set; }

        [JsonPropertyName("series")]
        public int Series { get; set; }

        [JsonPropertyName("transactions")]
        public int Transactions { get; set; }
    }

    public class AdminRecentActivityDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("date")]
        public string Date { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;
    }

    public class AdminDashboardResponseDto
    {
        [JsonPropertyName("stats")]
        public AdminDashboardStatsDto Stats { get; set; } = null!;

        [JsonPropertyName("recentActivities")]
        public List<AdminRecentActivityDto> RecentActivities { get; set; } = new();
    }
}
