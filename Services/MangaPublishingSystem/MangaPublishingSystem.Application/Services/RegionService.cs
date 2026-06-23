using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Regions;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class RegionService : GenericService<Region>, IRegionService
    {
        private readonly IPageRepository _pageRepository;

        public RegionService(
            IRegionRepository repository, 
            IPageRepository pageRepository,
            IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
            _pageRepository = pageRepository;
        }

        public async Task<Region> CreateRegionAsync(CreateRegionDto dto)
        {
            var page = await _pageRepository.GetByIdAsync(dto.PageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }

            var region = new Region
            {
                PageId = dto.PageId,
                Name = dto.Name,
                CoordinatesJson = dto.CoordinatesJson
            };

            await _repository.AddAsync(region);
            await _unitOfWork.SaveChangesAsync();

            return region;
        }

        public async Task<IEnumerable<Region>> GetRegionsByPageIdAsync(int pageId)
        {
            var page = await _pageRepository.GetByIdAsync(pageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }

            return await _repository.FindAsync(r => r.PageId == pageId);
        }

        public async Task<Region> UpdateRegionAsync(int id, UpdateRegionDto dto)
        {
            var region = await _repository.GetByIdAsync(id);
            if (region == null)
            {
                throw new NotFoundException("Phân vùng Canvas không tồn tại.");
            }

            region.CoordinatesJson = dto.CoordinatesJson;
            region.Name = dto.Name;

            _repository.Update(region);
            await _unitOfWork.SaveChangesAsync();

            return region;
        }

        public async Task DeleteRegionAsync(int id)
        {
            var region = await _repository.GetByIdAsync(id);
            if (region == null)
            {
                throw new NotFoundException("Phân vùng Canvas không tồn tại.");
            }

            _repository.Delete(region);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}