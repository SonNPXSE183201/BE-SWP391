using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class ReconciliationRecordDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("referenceCode")]
        public string ReferenceCode { get; set; } = null!;

        [JsonPropertyName("vnpayTransactionId")]
        public string VnpayTransactionId { get; set; } = null!;

        [JsonPropertyName("internalTransactionId")]
        public string InternalTransactionId { get; set; } = null!;

        [JsonPropertyName("vnpayAmount")]
        public decimal VnpayAmount { get; set; }

        [JsonPropertyName("internalAmount")]
        public decimal InternalAmount { get; set; }

        [JsonPropertyName("vnpayDate")]
        public string VnpayDate { get; set; } = null!;

        [JsonPropertyName("internalDate")]
        public string InternalDate { get; set; } = null!;

        [JsonPropertyName("vnpayStatus")]
        public string VnpayStatus { get; set; } = null!;

        [JsonPropertyName("internalStatus")]
        public string InternalStatus { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("discrepancyNote")]
        public string? DiscrepancyNote { get; set; }
    }
}
