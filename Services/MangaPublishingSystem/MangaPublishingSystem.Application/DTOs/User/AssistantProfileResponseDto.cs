using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.User
{
    public class AssistantProfileResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = null!;

        [JsonPropertyName("updatedAt")]
        public string? UpdatedAt { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;

        [JsonPropertyName("portfolioUrl")]
        public string? PortfolioUrl { get; set; }

        [JsonPropertyName("specialtyTags")]
        public List<string> SpecialtyTags { get; set; } = new();

        [JsonPropertyName("totalTasksCompleted")]
        public int TotalTasksCompleted { get; set; }

        [JsonPropertyName("averageRating")]
        public decimal AverageRating { get; set; }

        [JsonPropertyName("accountStatus")]
        public string AccountStatus { get; set; } = null!;
    }
}
