using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class CreateContractResponseDto
    {
        [JsonPropertyName("contractId")]
        public string ContractId { get; set; } = null!;

        [JsonPropertyName("seriesId")]
        public string SeriesId { get; set; } = null!;

        [JsonPropertyName("baseGenkouryoPrice")]
        public decimal BaseGenkouryoPrice { get; set; }
    }
}
