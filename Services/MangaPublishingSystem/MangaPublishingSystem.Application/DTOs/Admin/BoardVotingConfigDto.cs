namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class BoardVotingConfigDto
    {
        public int AutoResolveHours { get; set; } = 48;
        public int ApprovalThresholdPercent { get; set; } = 66;
        public int RejectionThresholdPercent { get; set; } = 66;
        public string TiePolicy { get; set; } = "Escalate";
        public bool ClearVotesOnResubmit { get; set; } = true;
        public bool RequireOddBoardSize { get; set; } = true;
        public int BoardRoleId { get; set; } = 3;
        public int? ChairUserId { get; set; }
        public string? ChairUserName { get; set; }
    }

    public class UpdateBoardVotingConfigDto
    {
        public int AutoResolveHours { get; set; }
        public int ApprovalThresholdPercent { get; set; }
        public int RejectionThresholdPercent { get; set; }
        public string TiePolicy { get; set; } = "Escalate";
        public bool ClearVotesOnResubmit { get; set; }
        public bool RequireOddBoardSize { get; set; }
        public int? ChairUserId { get; set; }
    }

    public class ManualResolveBoardVoteDto
    {
        public bool Approved { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal? ApprovedBudget { get; set; }
    }
}
