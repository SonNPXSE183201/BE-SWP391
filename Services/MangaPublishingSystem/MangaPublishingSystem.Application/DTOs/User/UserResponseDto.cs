using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int RoleId { get; set; }
        public string Status { get; set; } = null!;
        public string? PenName { get; set; }
        public string? Message { get; set; }
    }

    public class AssistantResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? PortfolioUrl { get; set; }
        public string? Skills { get; set; }
    }

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
    }

    public class ApproveAssistantRequestDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;

        [JsonPropertyName("approved")]
        public bool Approved { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }

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
