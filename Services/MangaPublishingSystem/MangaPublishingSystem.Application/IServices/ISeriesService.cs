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
        System.Threading.Tasks.Task SignContractAsync(int seriesId, int mangakaId);
        System.Threading.Tasks.Task DeclineFundAsync(int seriesId, int mangakaId);
        System.Threading.Tasks.Task VoteSeriesAsync(int seriesId, int boardUserId, string voteChoice, string comment, decimal recommendedBudget, string? publicationSchedule);
        Task<Chapter> SubmitChapterAsync(int seriesId, int mangakaId, SubmitChapterDto dto);
        Task<SeriesReviewDto> GetSeriesReviewAsync(int seriesId);
        Task<IEnumerable<SeriesReviewDto>> GetPendingReviewSeriesForEditorAsync(int editorId);
        System.Threading.Tasks.Task SubmitSeriesToBoardAsync(int seriesId, int editorId, SubmitToBoardDto dto);
        Task<IEnumerable<Series>> GetPendingBoardVoteSeriesAsync();
        System.Threading.Tasks.Task VoteRankingAsync(int seriesId, int boardUserId, string voteType, string? comment);
        System.Threading.Tasks.Task RequireSeriesRevisionAsync(int seriesId, int editorId, RequireSeriesRevisionDto dto);
        System.Threading.Tasks.Task UpdateSeriesAsync(int seriesId, int mangakaId, CreateSeriesDto dto);
        System.Threading.Tasks.Task DeleteSeriesAsync(int id, int currentUserId, string currentUserRole);
        Task<bool> HasContractAsync(int seriesId);
        Task<Contract?> GetContractBySeriesIdAsync(int seriesId);
    }
}
