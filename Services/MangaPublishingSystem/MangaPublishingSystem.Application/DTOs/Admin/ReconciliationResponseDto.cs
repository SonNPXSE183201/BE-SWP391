using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class ReconciliationResponseDto
    {
        [JsonPropertyName("records")]
        public List<ReconciliationRecordDto> Records { get; set; } = new();

        [JsonPropertyName("summary")]
        public ReconciliationSummaryDto Summary { get; set; } = null!;
    }
}
