using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Series;

namespace MangaPublishingSystem.Application.IServices
{
    public interface ISeriesService : IGenericService<Series>
    {
        Task<SeriesDto> CreateSeriesAsync(int mangakaId, CreateSeriesDto createDto);
        System.Threading.Tasks.Task SubmitForReviewAsync(int seriesId, int mangakaId, SubmitSeriesReviewDto submitDto);
        System.Threading.Tasks.Task SetAbsenceStatusAsync(int mangakaId, bool onLeave);
        Task<IEnumerable<SeriesDto>> GetSeriesByMangakaIdAsync(int mangakaId);
        System.Threading.Tasks.Task SubmitDraftManuscriptAsync(int seriesId, int mangakaId, string draftManuscriptUrl);
        System.Threading.Tasks.Task EvaluateSeriesByEditorAsync(int seriesId, int editorId, EditorEvaluationDto dto);
        System.Threading.Tasks.Task CastBoardVoteAsync(int seriesId, int boardMemberId, BoardVoteDto dto);
        System.Threading.Tasks.Task FinalizeBoardDecisionAsync(int seriesId, BoardDecisionDto dto);
    }
}