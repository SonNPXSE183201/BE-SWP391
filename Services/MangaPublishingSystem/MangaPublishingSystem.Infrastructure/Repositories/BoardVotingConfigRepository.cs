using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class BoardVotingConfigRepository : GenericRepository<BoardVotingConfig>, IBoardVotingConfigRepository
    {
        public BoardVotingConfigRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}
