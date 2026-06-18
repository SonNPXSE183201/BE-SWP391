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
}
