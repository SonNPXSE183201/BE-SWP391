using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class AssistantProfileRepository : GenericRepository<AssistantProfile>, IAssistantProfileRepository
    {
        public AssistantProfileRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}