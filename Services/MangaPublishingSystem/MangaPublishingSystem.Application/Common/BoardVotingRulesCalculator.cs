using System;
using System.Collections.Generic;
using System.Linq;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.Common
{
    public enum BoardVoteResolution
    {
        Pending,
        Approved,
        Rejected,
        Escalated
    }

    public sealed class BoardVotingThresholds
    {
        public int BoardMemberCount { get; init; }
        public int ApproveRequired { get; init; }
        public int RejectRequired { get; init; }
        public bool IsEvenBoardSize { get; init; }
        public string? OddBoardSizeWarning { get; init; }
    }

    public static class BoardVotingRulesCalculator
    {
        public const string TiePolicyReject = "Reject";
        public const string TiePolicyEscalate = "Escalate";
        public const string TiePolicyChairDecides = "ChairDecides";

        public static BoardVotingThresholds CalculateThresholds(int boardMemberCount, BoardVotingConfig config)
        {
            var n = Math.Max(boardMemberCount, 1);
            var approveRequired = CalcVotesRequired(n, config.ApprovalThresholdPercent);
            var rejectRequired = CalcVotesRequired(n, config.RejectionThresholdPercent);
            var isEven = n % 2 == 0;

            string? warning = null;
            if (config.RequireOddBoardSize && isEven)
            {
                warning =
                    $"Hội đồng đang có {n} thành viên (số chẵn). Khuyến nghị số lẻ để tránh hòa phiếu.";
            }

            return new BoardVotingThresholds
            {
                BoardMemberCount = n,
                ApproveRequired = approveRequired,
                RejectRequired = rejectRequired,
                IsEvenBoardSize = isEven,
                OddBoardSizeWarning = warning
            };
        }

        public static (int Approve, int Reject, int Abstain) CountVotes(IEnumerable<BoardVote> votes)
        {
            var approve = 0;
            var reject = 0;
            var abstain = 0;

            foreach (var vote in votes)
            {
                var type = vote.VoteType?.Trim() ?? string.Empty;
                if (type.Equals("Approve", StringComparison.OrdinalIgnoreCase)) approve++;
                else if (type.Equals("Reject", StringComparison.OrdinalIgnoreCase)) reject++;
                else if (type.Equals("Abstain", StringComparison.OrdinalIgnoreCase)) abstain++;
            }

            return (approve, reject, abstain);
        }

        public static BoardVoteResolution Evaluate(
            BoardVotingThresholds thresholds,
            int approveCount,
            int rejectCount,
            int abstainCount,
            int votesCast,
            BoardVotingConfig config,
            IEnumerable<BoardVote> votes)
        {
            if (approveCount >= thresholds.ApproveRequired)
            {
                return BoardVoteResolution.Approved;
            }

            if (rejectCount >= thresholds.RejectRequired)
            {
                return BoardVoteResolution.Rejected;
            }

            var allMembersVoted = votesCast >= thresholds.BoardMemberCount;
            if (!allMembersVoted)
            {
                return BoardVoteResolution.Pending;
            }

            if (approveCount == rejectCount)
            {
                return ResolveTie(config, votes);
            }

            // Tất cả đã vote nhưng không đạt ngưỡng và không hòa Approve/Reject → leo thang Admin
            return BoardVoteResolution.Escalated;
        }

        public static BoardVoteResolution EvaluateAutoResolve(
            BoardVotingThresholds thresholds,
            int approveCount,
            int rejectCount,
            BoardVotingConfig config,
            IEnumerable<BoardVote> votes)
        {
            if (approveCount >= thresholds.ApproveRequired)
            {
                return BoardVoteResolution.Approved;
            }

            if (rejectCount >= thresholds.RejectRequired)
            {
                return BoardVoteResolution.Rejected;
            }

            if (approveCount > rejectCount)
            {
                return BoardVoteResolution.Approved;
            }

            if (rejectCount > approveCount)
            {
                return BoardVoteResolution.Rejected;
            }

            return ResolveTie(config, votes);
        }

        private static BoardVoteResolution ResolveTie(BoardVotingConfig config, IEnumerable<BoardVote> votes)
        {
            var policy = config.TiePolicy?.Trim() ?? TiePolicyEscalate;

            if (policy.Equals(TiePolicyReject, StringComparison.OrdinalIgnoreCase))
            {
                return BoardVoteResolution.Rejected;
            }

            if (policy.Equals(TiePolicyChairDecides, StringComparison.OrdinalIgnoreCase) && config.ChairUserId.HasValue)
            {
                var chairVote = votes.FirstOrDefault(v => v.BoardMemberId == config.ChairUserId.Value);
                if (chairVote != null)
                {
                    var type = chairVote.VoteType?.Trim() ?? string.Empty;
                    if (type.Equals("Approve", StringComparison.OrdinalIgnoreCase))
                    {
                        return BoardVoteResolution.Approved;
                    }

                    if (type.Equals("Reject", StringComparison.OrdinalIgnoreCase))
                    {
                        return BoardVoteResolution.Rejected;
                    }
                }

                return BoardVoteResolution.Escalated;
            }

            return BoardVoteResolution.Escalated;
        }

        public static string NormalizeVoteType(string? voteChoice, bool legacyApproved, string? comment)
        {
            if (!string.IsNullOrWhiteSpace(voteChoice))
            {
                var choice = voteChoice.Trim();
                if (choice.Equals("Approve", StringComparison.OrdinalIgnoreCase)) return "Approve";
                if (choice.Equals("Reject", StringComparison.OrdinalIgnoreCase)) return "Reject";
                if (choice.Equals("Abstain", StringComparison.OrdinalIgnoreCase)) return "Abstain";
            }

            if (!string.IsNullOrWhiteSpace(comment) &&
                comment.TrimStart().StartsWith("[Abstain]", StringComparison.OrdinalIgnoreCase))
            {
                return "Abstain";
            }

            return legacyApproved ? "Approve" : "Reject";
        }

        /// <summary>
        /// Số phiếu tối thiểu để đạt ngưỡng %: ceil(N × p / 100) dùng phép nguyên (tránh lỗi float).
        /// Ví dụ N=6, p=67 → ceil(4.02)=5 vì 4/6=66.67% &lt; 67%. Preset 2/3 nên dùng p=66 → 4 phiếu.
        /// </summary>
        public static int CalcVotesRequired(int memberCount, int thresholdPercent)
        {
            var n = Math.Max(memberCount, 1);
            var percent = Math.Clamp(thresholdPercent, 1, 100);
            var product = n * percent;
            return Math.Max(1, (product + 99) / 100);
        }
    }
}
