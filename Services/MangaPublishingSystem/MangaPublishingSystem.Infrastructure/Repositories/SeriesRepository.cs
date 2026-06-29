using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class SeriesRepository : GenericRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(MangaPublishingDbContext context) : base(context)
        {
        }

        public async Task<bool> HasContractAsync(int seriesId)
        {
            return await _context.Series.AnyAsync(s => s.Id == seriesId && s.Contracts.Any());
        }

        public async Task<IReadOnlyList<Series>> FindWithDetailsAsync(Expression<Func<Series, bool>> predicate)
        {
            return await _context.Series
                .Include(s => s.Mangaka)
                .Include(s => s.Editor)
                .Where(predicate)
                .ToListAsync();
        }
    }
}