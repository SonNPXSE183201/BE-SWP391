using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ISeriesAssistantRepository : IGenericRepository<SeriesAssistant>
    {
        Task<IEnumerable<SeriesAssistant>> GetBySeriesIdAsync(int seriesId, string? status = null);
        Task<SeriesAssistant?> GetMembershipAsync(int seriesId, int assistantId);
        Task<bool> IsActiveMemberAsync(int seriesId, int assistantId);
        Task<IEnumerable<SeriesAssistant>> GetPendingInvitesByAssistantAsync(int assistantId);
    }
}
