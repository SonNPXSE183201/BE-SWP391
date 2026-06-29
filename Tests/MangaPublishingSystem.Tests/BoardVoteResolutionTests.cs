using MangaPublishingSystem.Application.Options;
using MangaPublishingSystem.Application.Services;
using Xunit;

namespace MangaPublishingSystem.Tests
{
    public class BoardVoteResolutionTests
    {
        private static BoardVoteSettings DefaultSettings => new()
        {
            Mode = "Auto",
            RequireOddActiveBoardMemberCount = true,
            VoteDeadlineHours = 72,
            OnDeadline = "ResolveByCastMajority"
        };

        [Fact]
        public void OddBoard_SevenMembers_FourApprove_PassesWithoutAllVotes()
        {
            var thresholds = BoardVoteOddMajorityRules.GetThresholds(7, DefaultSettings);
            Assert.Equal(4, thresholds.ApproveThreshold);

            var outcome = BoardVoteOddMajorityRules.Evaluate(new BoardVoteResolutionInput(
                7, 4, 0, 4, null, DefaultSettings));

            Assert.Equal(BoardVoteOutcome.Approved, outcome);
        }

        [Fact]
        public void OddBoard_SevenMembers_ThreeApproveThreeReject_StillPending()
        {
            var outcome = BoardVoteOddMajorityRules.Evaluate(new BoardVoteResolutionInput(
                7, 3, 3, 6, null, DefaultSettings));

            Assert.Equal(BoardVoteOutcome.Pending, outcome);
        }

        [Fact]
        public void EvenBoard_SixMembers_ThrowsWhenRequiredOdd()
        {
            var ex = Assert.Throws<BuildingBlocks.Exceptions.BadRequestException>(() =>
                BoardVoteOddMajorityRules.EnsureOddBoardMemberCount(6, DefaultSettings));

            Assert.Contains("số lẻ", ex.Message);
        }

        [Fact]
        public void ManualMode_UsesConfiguredThresholds()
        {
            var manual = new BoardVoteSettings
            {
                Mode = "Manual",
                ManualApproveThreshold = 5,
                ManualRejectThreshold = 5
            };

            var thresholds = BoardVoteOddMajorityRules.GetThresholds(7, manual);
            Assert.Equal(5, thresholds.ApproveThreshold);
        }

        [Fact]
        public void Deadline_MajorityApprove_Wins()
        {
            var started = DateTime.UtcNow.AddHours(-80);
            var outcome = BoardVoteOddMajorityRules.Evaluate(new BoardVoteResolutionInput(
                7, 3, 1, 4, started, DefaultSettings));

            Assert.Equal(BoardVoteOutcome.Approved, outcome);
        }
    }
}
