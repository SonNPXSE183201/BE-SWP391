using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.DTOs.Chapters;
using MangaPublishingSystem.Application.DTOs.Reviews;

namespace MangaPublishingSystem.Application.IServices
{
    public interface ISeriesService : IGenericService<Series>
    {
        Task<Series> CreateSeriesAsync(int mangakaId, CreateSeriesDto createDto);
        System.Threading.Tasks.Task SubmitForReviewAsync(int seriesId, int mangakaId, SubmitSeriesReviewDto submitDto);
        System.Threading.Tasks.Task SetAbsenceStatusAsync(int mangakaId, bool onLeave);
        Task<IEnumerable<Series>> GetSeriesByMangakaIdAsync(int mangakaId);
        System.Threading.Tasks.Task AcceptFundAsync(int seriesId, int mangakaId);
        System.Threading.Tasks.Task VoteSeriesAsync(int seriesId, int boardUserId, bool approved, string comment, decimal recommendedBudget);
        Task<Chapter> SubmitChapterAsync(int seriesId, int mangakaId, SubmitChapterDto dto);
        Task<SeriesReviewDto> GetSeriesReviewAsync(int seriesId);
        System.Threading.Tasks.Task SubmitSeriesToBoardAsync(int seriesId, int editorId, SubmitToBoardDto dto);
        Task<IEnumerable<Series>> GetPendingBoardVoteSeriesAsync();
        System.Threading.Tasks.Task VoteRankingAsync(int seriesId, int boardUserId, string voteType, string? comment);
        System.Threading.Tasks.Task RequireSeriesRevisionAsync(int seriesId, int editorId, string comment);
        System.Threading.Tasks.Task UpdateSeriesAsync(int seriesId, int mangakaId, CreateSeriesDto dto);
    }
}

