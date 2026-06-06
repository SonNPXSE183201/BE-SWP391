using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class RankingRecordRepository : GenericRepository<RankingRecord>, IRankingRecordRepository
    {
        public RankingRecordRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}