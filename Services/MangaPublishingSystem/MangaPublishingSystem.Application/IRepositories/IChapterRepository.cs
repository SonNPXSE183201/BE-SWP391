using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IChapterRepository : IGenericRepository<Chapter>
    {
        Task<IEnumerable<Chapter>> GetPendingReviewChaptersWithDetailsAsync(int editorId);
        Task<Chapter?> GetChapterWithDetailsByIdAsync(int id);
    }
}