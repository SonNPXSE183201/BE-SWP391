using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Pages;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IPageService : IGenericService<Page>
    {
        Task<IEnumerable<Page>> GetPagesByChapterIdAsync(int chapterId);
        Task<IEnumerable<LayerDto>> GetPageLayersAsync(int pageId);
    }
}