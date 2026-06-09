using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Series;

namespace MangaPublishingSystem.Application.IServices
{
    public interface ISeriesService : IGenericService<Series>
    {
        Task<Series> CreateSeriesAsync(int mangakaId, CreateSeriesDto createDto);
        System.Threading.Tasks.Task SubmitForReviewAsync(int seriesId, int mangakaId, SubmitSeriesReviewDto submitDto);
        System.Threading.Tasks.Task SetAbsenceStatusAsync(int mangakaId, bool onLeave);
        Task<IEnumerable<Series>> GetSeriesByMangakaIdAsync(int mangakaId);
    }
}