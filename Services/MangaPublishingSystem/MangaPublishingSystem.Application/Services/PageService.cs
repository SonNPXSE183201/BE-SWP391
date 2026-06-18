using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Application.DTOs.Pages;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class PageService : GenericService<Page>, IPageService
    {
        public PageService(IPageRepository repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
        }

        public async Task<IEnumerable<Page>> GetPagesByChapterIdAsync(int chapterId)
        {
            return await _repository.FindAsync(p => p.ChapterId == chapterId);
        }

        public async Task<IEnumerable<LayerDto>> GetPageLayersAsync(int pageId)
        {
            var pageRepo = (IPageRepository)_repository;
            var page = await pageRepo.GetByIdAsync(pageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }
            return await pageRepo.GetPageLayersAsync(pageId);
        }
    }
}