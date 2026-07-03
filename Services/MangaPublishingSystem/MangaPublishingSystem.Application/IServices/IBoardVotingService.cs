using System.Collections.Generic;
using MangaPublishingSystem.Application.Common;
using MangaPublishingSystem.Application.DTOs.Admin;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IBoardVotingService
    {
        System.Threading.Tasks.Task<BoardVotingConfig> GetConfigAsync();
        System.Threading.Tasks.Task<BoardVotingConfigDto> GetConfigDtoAsync();
        System.Threading.Tasks.Task<BoardVotingConfigDto> UpdateConfigAsync(UpdateBoardVotingConfigDto dto);
        System.Threading.Tasks.Task<BoardVotingRulesDto> BuildRulesDtoAsync();
        System.Threading.Tasks.Task<PendingBoardVotesResponseDto> GetPendingVotesPayloadAsync(int boardMemberId);
        System.Threading.Tasks.Task<IEnumerable<SeriesDto>> GetEscalatedSeriesAsync();
        System.Threading.Tasks.Task ManualResolveAsync(int seriesId, int adminUserId, ManualResolveBoardVoteDto dto);
        System.Threading.Tasks.Task ClearVotesForSeriesAsync(int seriesId);
        System.Threading.Tasks.Task<BoardVoteResolution> EvaluateSeriesVotesAsync(int seriesId);
        System.Threading.Tasks.Task ApplyVoteResolutionAsync(Series series, BoardVoteResolution resolution, string? rejectComment = null);
        System.Threading.Tasks.Task ClearChairIfUserDeactivatedAsync(int userId);
        System.Threading.Tasks.Task NotifyBoardMembershipChangedAsync(int userId);
    }
}
