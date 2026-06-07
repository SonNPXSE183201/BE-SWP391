using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Infrastructure.Data;

namespace MangaPublishingSystem.Infrastructure.Repositories
{
    public class BoardVoteRepository : GenericRepository<BoardVote>, IBoardVoteRepository
    {
        public BoardVoteRepository(MangaPublishingDbContext context) : base(context)
        {
        }
    }
}