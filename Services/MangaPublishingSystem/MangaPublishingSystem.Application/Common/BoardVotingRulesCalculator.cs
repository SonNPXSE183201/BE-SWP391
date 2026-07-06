using System;
using System.Collections.Generic;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.Common
{
    public enum BoardVoteResolution
    {
        Pending,
        Approved,
        Rejected
    }

    public sealed class BoardVotingThresholds
    {
        public int BoardMemberCount { get; init; }
        public int ChairWeight { get; init; }
        public int TotalWeight { get; init; }
        public int ApproveRequired { get; init; }
    }

    public static class BoardVotingRulesCalculator
    {
        public static BoardVotingThresholds CalculateThresholds(int boardMemberCount, BoardVotingConfig config)
        {
            if (boardMemberCount < 3)
            {
                throw new InvalidOperationException("Hội đồng phải có ít nhất 3 thành viên (N >= 3).");
            }

            var n = boardMemberCount;
            var originalChairWeight = (n % 2 == 0) ? 2 : 3;
            var chairWeight = Math.Min(originalChairWeight, n - 2);
            var totalWeight = chairWeight + (n - 1);
            var approveRequired = CalcVotesRequired(totalWeight, config.ApprovalThresholdPercent);

            return new BoardVotingThresholds
            {
                BoardMemberCount = n,
                ChairWeight = chairWeight,
                TotalWeight = totalWeight,
                ApproveRequired = approveRequired
            };
        }

        public static (int ApproveWeight, int RejectWeight) CountWeightedVotes(
            IEnumerable<BoardVote> votes, int? chairUserId, int chairWeight)
        {
            var approve = 0;
            var reject = 0;

            foreach (var vote in votes)
            {
                var weight = (chairUserId.HasValue && vote.BoardMemberId == chairUserId.Value)
                    ? chairWeight
                    : 1;
                var type = vote.VoteType?.Trim() ?? string.Empty;
                if (type.Equals("Approve", StringComparison.OrdinalIgnoreCase))
                {
                    approve += weight;
                }
                else if (type.Equals("Reject", StringComparison.OrdinalIgnoreCase))
                {
                    reject += weight;
                }
            }

            return (approve, reject);
        }

        public static BoardVoteResolution Evaluate(
            BoardVotingThresholds thresholds,
            int approveWeight,
            int rejectWeight,
            int votesCast,
            int totalMembers)
        {
            // Bug-005: Đợi đủ tất cả thành viên vote xong mới chốt kết quả (trừ khi tự động chốt do hết hạn)
            if (votesCast < totalMembers)
            {
                return BoardVoteResolution.Pending;
            }

            if (approveWeight >= thresholds.ApproveRequired)
            {
                return BoardVoteResolution.Approved;
            }

            return BoardVoteResolution.Rejected;
        }

        public static BoardVoteResolution EvaluateAutoResolve(
            BoardVotingThresholds thresholds,
            int approveWeight,
            int rejectWeight)
        {
            if (approveWeight > rejectWeight)
            {
                return BoardVoteResolution.Approved;
            }

            return BoardVoteResolution.Rejected;
        }

        public static decimal CalculateApprovedBudget(
            IEnumerable<BoardVote> votes, int? chairUserId, int chairWeight)
        {
            decimal totalBudgetWeight = 0;
            var totalApproveWeight = 0;

            foreach (var vote in votes)
            {
                var type = vote.VoteType?.Trim() ?? string.Empty;
                if (!type.Equals("Approve", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var weight = (chairUserId.HasValue && vote.BoardMemberId == chairUserId.Value)
                    ? chairWeight
                    : 1;
                totalBudgetWeight += vote.RecommendedBudget * weight;
                totalApproveWeight += weight;
            }

            if (totalApproveWeight == 0)
            {
                return 0;
            }

            var rawBudget = totalBudgetWeight / totalApproveWeight;
            return Math.Round(rawBudget / 1000m) * 1000m;
        }

        public static string NormalizeVoteType(string? voteChoice, bool legacyApproved, string? comment)
        {
            if (!string.IsNullOrWhiteSpace(voteChoice))
            {
                var choice = voteChoice.Trim();
                if (choice.Equals("Approve", StringComparison.OrdinalIgnoreCase)) return "Approve";
                if (choice.Equals("Reject", StringComparison.OrdinalIgnoreCase)) return "Reject";
            }

            return legacyApproved ? "Approve" : "Reject";
        }

        /// <summary>
        /// Số phiếu tối thiểu để đạt ngưỡng %: ceil(N × p / 100) dùng phép nguyên (tránh lỗi float).
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
