using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class TasksRepository : GenericRepository<Tasks>, ITasksRepository
    {
        public TasksRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}