using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class TaskVersionRepository : GenericRepository<TaskVersion>, ITaskVersionRepository
    {
        public TaskVersionRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}