using System.Collections.Generic;

namespace MangaPublishingSystem.Application.DTOs.Wallet
{
    public class ReconciliationReportDto
    {
        public int TotalRows { get; set; }
        public int MatchedCount { get; set; }
        public int ResolvedCount { get; set; }
        public int UnresolvedCount { get; set; }
        public List<string> Details { get; set; } = new List<string>();
    }
}
