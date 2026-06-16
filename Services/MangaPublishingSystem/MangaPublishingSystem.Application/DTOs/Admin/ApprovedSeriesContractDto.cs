using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class ApprovedSeriesContractDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("mangakaName")]
        public string MangakaName { get; set; } = null!;

        [JsonPropertyName("approvedAt")]
        public string ApprovedAt { get; set; } = null!;

        [JsonPropertyName("approvedBudget")]
        public decimal ApprovedBudget { get; set; }

        [JsonPropertyName("publishSchedule")]
        public string PublishSchedule { get; set; } = null!;

        [JsonPropertyName("hasContract")]
        public bool HasContract { get; set; }

        [JsonPropertyName("genres")]
        public List<string> Genres { get; set; } = new();
    }
}
