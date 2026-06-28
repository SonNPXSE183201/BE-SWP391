using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface ISeriesRepository : IGenericRepository<Series>
    {
        Task<bool> HasContractAsync(int seriesId);
    }
}