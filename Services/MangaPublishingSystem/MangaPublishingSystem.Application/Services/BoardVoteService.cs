using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class BoardVoteService : GenericService<BoardVote>, IBoardVoteService
    {
        public BoardVoteService(IBoardVoteRepository repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
        }
    }
}