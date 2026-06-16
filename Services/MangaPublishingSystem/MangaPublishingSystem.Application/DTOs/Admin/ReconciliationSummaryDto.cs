using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class ReconciliationSummaryDto
    {
        [JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }

        [JsonPropertyName("matchedCount")]
        public int MatchedCount { get; set; }

        [JsonPropertyName("mismatchCount")]
        public int MismatchCount { get; set; }

        [JsonPropertyName("missingCount")]
        public int MissingCount { get; set; }

        [JsonPropertyName("pendingCount")]
        public int PendingCount { get; set; }

        [JsonPropertyName("totalVnpayAmount")]
        public decimal TotalVnpayAmount { get; set; }

        [JsonPropertyName("totalInternalAmount")]
        public decimal TotalInternalAmount { get; set; }

        [JsonPropertyName("differenceAmount")]
        public decimal DifferenceAmount { get; set; }
    }
}
