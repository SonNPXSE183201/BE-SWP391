using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class SeriesRepository : GenericRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}