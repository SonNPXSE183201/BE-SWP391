using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class ChapterRepository : GenericRepository<Chapter>, IChapterRepository
    {
        public ChapterRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}