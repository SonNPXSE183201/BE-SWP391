using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MangaPublishingSystem.Application.DTOs.Chapters;
using MangaPublishingSystem.Application.DTOs.Publishing;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IChapterService : IGenericService<Chapter>
    {
        System.Threading.Tasks.Task ApproveChapterAsync(int chapterId, int editorId, DTOs.Reviews.ApproveChapterDto dto);
        System.Threading.Tasks.Task RejectChapterAsync(int chapterId, int editorId, DTOs.Reviews.RejectChapterDto dto);
        Task<IEnumerable<Chapter>> GetPendingReviewChaptersForEditorAsync(int editorId);
        Task<IEnumerable<PublishingScheduleDto>> GetPublishingScheduleAsync(string month);
        System.Threading.Tasks.Task UpdateDeadlineAsync(int chapterId, DateTime deadline);
        System.Threading.Tasks.Task PublishChapterAsync(int chapterId);
        Task<IEnumerable<Chapter>> GetChaptersBySeriesIdAsync(int seriesId);

        /// <summary>
        /// Nối thêm các trang ảnh vào một chapter đã tồn tại (Upload thêm trang).
        /// Trả về danh sách các trang vừa được thêm.
        /// </summary>
        Task<IEnumerable<Page>> AddPagesAsync(int chapterId, int mangakaId, List<IFormFile> files);

        Task<ChapterProductionReadinessDto> GetProductionReadinessAsync(int chapterId, int mangakaId);

        System.Threading.Tasks.Task<Chapter> SubmitChapterForReviewAsync(int chapterId, int mangakaId);

        Task<Chapter?> GetChapterWithDetailsAsync(int chapterId);

        /// <summary>
        /// Đánh dấu trang không cần sản xuất Assistant (ảnh upload đã hoàn chỉnh).
        /// </summary>
        Task<Page> MarkPageAsReadyAsync(int pageId, int mangakaId);

        Task<Page> UnmarkPageAsReadyAsync(int pageId, int mangakaId);

        /// <summary>
        /// Thay thế ảnh bản thảo của một trang đã tồn tại (Mangaka tự sửa sau feedback Editor).
        /// </summary>
        Task<Page> ReplacePageImageAsync(int pageId, int mangakaId, IFormFile file);
    }
}
