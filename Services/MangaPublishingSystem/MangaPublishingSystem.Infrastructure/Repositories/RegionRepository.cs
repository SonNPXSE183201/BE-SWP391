using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class RegionRepository : GenericRepository<Region>, IRegionRepository
    {
        public RegionRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<Region?> GetByIdWithPageChapterSeriesAsync(int id)
        {
            return await _dbSet.AsQueryable()
                .Include(r => r.Page)
                    .ThenInclude(p => p.Chapter)
                        .ThenInclude(c => c.Series)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}