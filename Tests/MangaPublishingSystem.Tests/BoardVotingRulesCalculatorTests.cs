using System.Collections.Generic;
using MangaPublishingSystem.Application.Common;
using MangaPublishingSystem.Domain.Entities;
using Xunit;

namespace MangaPublishingSystem.Tests
{
    public class BoardVotingRulesCalculatorTests
    {
        [Fact]
        public void CalculateApprovedBudget_ChairWeight3_MemberWeight1_Returns25M()
        {
            var votes = new List<BoardVote>
            {
                new() { BoardMemberId = 1, VoteType = "Approve", RecommendedBudget = 30_000_000m },
                new() { BoardMemberId = 2, VoteType = "Approve", RecommendedBudget = 10_000_000m },
            };

            var result = BoardVotingRulesCalculator.CalculateApprovedBudget(votes, chairUserId: 1, chairWeight: 3);

            Assert.Equal(25_000_000m, result);
        }

        [Fact]
        public void CalculateApprovedBudget_EqualWeights_Returns10M()
        {
            var votes = new List<BoardVote>
            {
                new() { BoardMemberId = 1, VoteType = "Approve", RecommendedBudget = 10_000_000m },
                new() { BoardMemberId = 2, VoteType = "Approve", RecommendedBudget = 10_000_000m },
                new() { BoardMemberId = 3, VoteType = "Approve", RecommendedBudget = 10_000_000m },
            };

            var result = BoardVotingRulesCalculator.CalculateApprovedBudget(votes, chairUserId: 1, chairWeight: 1);

            Assert.Equal(10_000_000m, result);
        }

        [Fact]
        public void CalculateApprovedBudget_RoundsToNearestThousand()
        {
            var votes = new List<BoardVote>
            {
                new() { BoardMemberId = 1, VoteType = "Approve", RecommendedBudget = 20_000_000m },
                new() { BoardMemberId = 2, VoteType = "Approve", RecommendedBudget = 10_000_000m },
            };

            var result = BoardVotingRulesCalculator.CalculateApprovedBudget(votes, chairUserId: 1, chairWeight: 2);

            Assert.Equal(16_667_000m, result);
        }

        [Fact]
        public void CalculateThresholds_SixMembers_TotalWeightIsOdd()
        {
            var config = new BoardVotingConfig { ApprovalThresholdPercent = 51 };
            var thresholds = BoardVotingRulesCalculator.CalculateThresholds(6, config);

            Assert.Equal(2, thresholds.ChairWeight);
            Assert.Equal(7, thresholds.TotalWeight);
            Assert.Equal(4, thresholds.ApproveRequired);
        }
    }
}
