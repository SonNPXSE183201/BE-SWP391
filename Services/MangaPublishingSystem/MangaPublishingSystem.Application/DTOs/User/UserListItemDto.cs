using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.User
{
    public class UserListItemDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = null!;

        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = null!;

        [JsonPropertyName("assignedEditorId")]
        public int? AssignedEditorId { get; set; }

        [JsonPropertyName("assignedEditorName")]
        public string? AssignedEditorName { get; set; }
    }
}
