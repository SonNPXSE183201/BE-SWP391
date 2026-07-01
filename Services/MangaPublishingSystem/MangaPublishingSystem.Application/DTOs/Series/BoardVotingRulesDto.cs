namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class BoardVotingRulesDto
    {
        public int BoardMemberCount { get; set; }
        public int ApproveRequired { get; set; }
        public int TotalWeight { get; set; }
        public int ChairWeight { get; set; }
        public int ApprovalThresholdPercent { get; set; }
        public int AutoResolveHours { get; set; }
        public int? ChairUserId { get; set; }
        public string? ChairUserName { get; set; }
        public bool ChairIsValid { get; set; } = true;
        public string? ChairInvalidWarning { get; set; }
        public int? EffectiveChairUserId { get; set; }
        public string RulesSummary { get; set; } = string.Empty;
    }
}
