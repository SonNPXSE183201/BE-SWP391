using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class AdminDashboardResponseDto
    {
        [JsonPropertyName("stats")]
        public AdminDashboardStatsDto Stats { get; set; } = null!;

        [JsonPropertyName("recentActivities")]
        public List<AdminRecentActivityDto> RecentActivities { get; set; } = new();
    }
}
