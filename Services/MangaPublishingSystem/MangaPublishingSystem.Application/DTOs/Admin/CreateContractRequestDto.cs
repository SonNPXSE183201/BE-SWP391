using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class CreateContractRequestDto
    {
        [JsonPropertyName("seriesId")]
        public string SeriesId { get; set; } = null!;

        [JsonPropertyName("baseGenkouryoPrice")]
        public decimal BaseGenkouryoPrice { get; set; }
    }
}
