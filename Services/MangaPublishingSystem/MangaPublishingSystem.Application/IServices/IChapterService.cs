using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IChapterService : IGenericService<Chapter>
    {
        System.Threading.Tasks.Task ApproveChapterAsync(int chapterId, int editorId, DTOs.Reviews.ApproveChapterDto dto);
        System.Threading.Tasks.Task RejectChapterAsync(int chapterId, int editorId, DTOs.Reviews.RejectChapterDto dto);
        Task<IEnumerable<Chapter>> GetPendingReviewChaptersAsync();
        System.Threading.Tasks.Task UpdateDeadlineAsync(int chapterId, DateTime deadline);
        System.Threading.Tasks.Task PublishChapterAsync(int chapterId);
        Task<IEnumerable<Chapter>> GetChaptersBySeriesIdAsync(int seriesId);
    }
}