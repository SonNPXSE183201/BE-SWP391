using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Series;

namespace MangaPublishingSystem.Application.IServices
{
    public interface ISeriesTeamService
    {
        Task<IEnumerable<SeriesAssistantDto>> GetTeamMembersAsync(int seriesId, int userId, bool isMangaka);
        Task<IEnumerable<SeriesAssistantDto>> GetActiveTeamForAssignmentAsync(int seriesId, int mangakaId);
        Task<SeriesAssistantDto> InviteAssistantAsync(int seriesId, int mangakaId, InviteSeriesAssistantDto dto);
        Task<SeriesAssistantDto> RespondToInviteAsync(int seriesId, int assistantId, RespondSeriesInviteDto dto);
        System.Threading.Tasks.Task RemoveMemberAsync(int seriesId, int mangakaId, int assistantId, string? roleToRemove = null);
    }
}
