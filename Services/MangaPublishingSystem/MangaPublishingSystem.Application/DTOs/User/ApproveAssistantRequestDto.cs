using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.User
{
    public class ApproveAssistantRequestDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;

        [JsonPropertyName("approved")]
        public bool Approved { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}
