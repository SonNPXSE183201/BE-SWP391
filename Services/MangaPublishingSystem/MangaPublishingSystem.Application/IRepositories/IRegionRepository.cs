using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IRegionRepository : IGenericRepository<Region>
    {
        Task<Region?> GetByIdWithPageChapterSeriesAsync(int id);
    }
}