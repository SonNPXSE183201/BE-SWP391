using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class SeriesAssistantRepository : GenericRepository<SeriesAssistant>, ISeriesAssistantRepository
    {
        public SeriesAssistantRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SeriesAssistant>> GetBySeriesIdAsync(int seriesId, string? status = null)
        {
            var query = _dbSet.AsQueryable()
                .Include(sa => sa.Assistant)
                .Where(sa => sa.SeriesId == seriesId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(sa => sa.Status == status);
            }

            return await query
                .OrderByDescending(sa => sa.Status == "Active")
                .ThenBy(sa => sa.Assistant.FullName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SeriesAssistant?> GetMembershipAsync(int seriesId, int assistantId)
        {
            return await _dbSet.AsQueryable()
                .Include(sa => sa.Assistant)
                .Include(sa => sa.Series)
                .FirstOrDefaultAsync(sa => sa.SeriesId == seriesId && sa.AssistantId == assistantId);
        }

        public async Task<bool> IsActiveMemberAsync(int seriesId, int assistantId)
        {
            return await _dbSet.AsQueryable()
                .AnyAsync(sa => sa.SeriesId == seriesId
                    && sa.AssistantId == assistantId
                    && sa.Status == "Active");
        }
    }
}
