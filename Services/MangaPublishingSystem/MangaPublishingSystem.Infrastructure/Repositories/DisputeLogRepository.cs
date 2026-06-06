using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class DisputeLogRepository : GenericRepository<DisputeLog>, IDisputeLogRepository
    {
        public DisputeLogRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}