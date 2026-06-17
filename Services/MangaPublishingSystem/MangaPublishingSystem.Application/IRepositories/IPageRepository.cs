using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Pages;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IPageRepository : IGenericRepository<Page>
    {
        Task<List<LayerDto>> GetPageLayersAsync(int pageId);
    }
}