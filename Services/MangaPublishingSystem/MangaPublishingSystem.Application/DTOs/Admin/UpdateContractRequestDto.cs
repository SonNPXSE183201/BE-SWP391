using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class UpdateContractRequestDto
    {
        [JsonPropertyName("contractId")]
        public string ContractId { get; set; } = null!;

        [JsonPropertyName("genkouryoPrice")]
        public decimal? GenkouryoPrice { get; set; }

        [JsonPropertyName("endDate")]
        public string? EndDate { get; set; }
    }
}
