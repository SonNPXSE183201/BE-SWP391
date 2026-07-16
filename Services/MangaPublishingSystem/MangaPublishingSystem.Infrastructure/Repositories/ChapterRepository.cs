using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class ChapterRepository : GenericRepository<Chapter>, IChapterRepository
    {
        private readonly MangaPublishingDbContext _dbContext;

        public ChapterRepository(MangaPublishingDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<IEnumerable<Chapter>> GetPendingReviewChaptersWithDetailsAsync(int editorId)
        {
            return await _dbContext.Chapters
                .Where(c => c.Status == "Pending_Review" && c.Series != null && c.Series.EditorId == editorId)
                .Include(c => c.Series)
                    .ThenInclude(s => s.Mangaka)
                .Include(c => c.Pages)
                    .ThenInclude(p => p.Annotations)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Chapter?> GetChapterWithDetailsByIdAsync(int id)
        {
            return await _dbContext.Chapters
                .Where(c => c.Id == id)
                .Include(c => c.Series)
                    .ThenInclude(s => s.Mangaka)
                .Include(c => c.Pages)
                    .ThenInclude(p => p.Annotations)
                .Include(c => c.Pages)
                    .ThenInclude(p => p.Regions)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Chapter>> GetPublishingScheduleAsync(DateTime monthStart, DateTime monthEnd)
        {
            return await _dbContext.Chapters
                .Where(c =>
                    c.PublishDate.HasValue &&
                    c.PublishDate.Value >= monthStart &&
                    c.PublishDate.Value < monthEnd)
                .Include(c => c.Series)
                    .ThenInclude(s => s.Mangaka)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
