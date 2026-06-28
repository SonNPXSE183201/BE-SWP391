namespace MangaPublishingSystem.Application.DTOs.Series
{
    public class BoardVotingRulesDto
    {
        public int BoardMemberCount { get; set; }
        public int ApproveRequired { get; set; }
        public int RejectRequired { get; set; }
        public int ApprovalThresholdPercent { get; set; }
        public int RejectionThresholdPercent { get; set; }
        public string TiePolicy { get; set; } = "Escalate";
        public int AutoResolveHours { get; set; }
        public bool IsEvenBoardSize { get; set; }
        public bool RequireOddBoardSize { get; set; }
        public string? OddBoardSizeWarning { get; set; }
        public int? ChairUserId { get; set; }
        public string? ChairUserName { get; set; }
        public string RulesSummary { get; set; } = string.Empty;
    }
}
