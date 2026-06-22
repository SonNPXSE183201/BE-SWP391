using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class PortfolioSampleRepository : GenericRepository<PortfolioSample>, IPortfolioSampleRepository
    {
        public PortfolioSampleRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}
