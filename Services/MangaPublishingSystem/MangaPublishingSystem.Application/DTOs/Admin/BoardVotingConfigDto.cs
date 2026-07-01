namespace MangaPublishingSystem.Application.DTOs.Admin
{
    public class BoardVotingConfigDto
    {
        public int AutoResolveHours { get; set; } = 48;
        public int ApprovalThresholdPercent { get; set; } = 51;
        public bool ClearVotesOnResubmit { get; set; } = true;
        public int BoardRoleId { get; set; } = 3;
        public int? ChairUserId { get; set; }
        public string? ChairUserName { get; set; }
        public bool ChairIsValid { get; set; } = true;
        public string? ChairInvalidWarning { get; set; }
    }

    public class UpdateBoardVotingConfigDto
    {
        public int AutoResolveHours { get; set; }
        public int ApprovalThresholdPercent { get; set; }
        public bool ClearVotesOnResubmit { get; set; }
        public int? ChairUserId { get; set; }
    }

    public class ManualResolveBoardVoteDto
    {
        public bool Approved { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal? ApprovedBudget { get; set; }
    }
}
