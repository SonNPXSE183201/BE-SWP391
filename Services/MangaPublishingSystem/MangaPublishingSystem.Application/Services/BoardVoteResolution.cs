using System;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.Options;

namespace MangaPublishingSystem.Application.Services
{
    public enum BoardVoteOutcome
    {
        Pending,
        Approved,
        Rejected,
        EscalateToEditor
    }

    public sealed record BoardVoteResolutionInput(
        int ActiveBoardMemberCount,
        int ApproveCount,
        int RejectCount,
        int TotalVotesCast,
        DateTime? BoardVoteStartedAtUtc,
        BoardVoteSettings Settings);

    public sealed record BoardVoteThresholds(int ApproveThreshold, int RejectThreshold);

    public static class BoardVoteResolution
    {
        /// <summary>
        /// N lẻ → ngưỡng duyệt/từ chối = ⌊N/2⌋+1 (vd. N=7 → cần 4 phiếu).
        /// Không cần vote đủ 7 người: 4 Approve là đủ dù 3 người chưa vote.
        /// </summary>
        public static BoardVoteThresholds GetThresholds(int n, BoardVoteSettings settings)
        {
            if (n <= 0)
            {
                n = 1;
            }

            if (string.Equals(settings.Mode, "Manual", StringComparison.OrdinalIgnoreCase)
                && settings.ManualApproveThreshold is > 0
                && settings.ManualRejectThreshold is > 0)
            {
                return new BoardVoteThresholds(settings.ManualApproveThreshold.Value, settings.ManualRejectThreshold.Value);
            }

            var majorityThreshold = (n / 2) + 1;
            return new BoardVoteThresholds(majorityThreshold, majorityThreshold);
        }

        public static void EnsureOddBoardMemberCount(int activeBoardMemberCount, BoardVoteSettings settings)
        {
            if (!settings.RequireOddActiveBoardMemberCount)
            {
                return;
            }

            if (activeBoardMemberCount <= 0)
            {
                throw new BadRequestException("Hệ thống chưa có thành viên Hội đồng biên tập đang hoạt động.");
            }

            if (activeBoardMemberCount % 2 == 0)
            {
                throw new BadRequestException(
                    $"Số thành viên Hội đồng phải là số lẻ (hiện có {activeBoardMemberCount} thành viên Active). " +
                    "Admin cần thêm hoặc khóa bớt một tài khoản Editorial Board trước khi biểu quyết.");
            }
        }

        public static BoardVoteOutcome Evaluate(BoardVoteResolutionInput input)
        {
            var n = input.ActiveBoardMemberCount;
            var thresholds = GetThresholds(n, input.Settings);

            if (input.ApproveCount >= thresholds.ApproveThreshold)
            {
                return BoardVoteOutcome.Approved;
            }

            if (input.RejectCount >= thresholds.RejectThreshold)
            {
                return BoardVoteOutcome.Rejected;
            }

            if (IsDeadlineExpired(input.BoardVoteStartedAtUtc, input.Settings))
            {
                return input.Settings.OnDeadline switch
                {
                    "Reject" => BoardVoteOutcome.Rejected,
                    "EscalateToEditor" => BoardVoteOutcome.EscalateToEditor,
                    _ => ResolveByCastMajority(input.ApproveCount, input.RejectCount)
                };
            }

            return BoardVoteOutcome.Pending;
        }

        private static bool IsDeadlineExpired(DateTime? startedAtUtc, BoardVoteSettings settings)
        {
            if (settings.VoteDeadlineHours <= 0 || startedAtUtc is null)
            {
                return false;
            }

            return DateTime.UtcNow >= startedAtUtc.Value.AddHours(settings.VoteDeadlineHours);
        }

        /// <summary>Bên có nhiều phiếu hơn thắng; hòa thì từ chối (chỉ xảy ra khi hết hạn, không phải 50-50 đủ N lẻ).</summary>
        private static BoardVoteOutcome ResolveByCastMajority(int approveCount, int rejectCount)
        {
            if (approveCount > rejectCount)
            {
                return BoardVoteOutcome.Approved;
            }

            if (rejectCount > approveCount)
            {
                return BoardVoteOutcome.Rejected;
            }

            return BoardVoteOutcome.Rejected;
        }
    }
}
