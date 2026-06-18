using System.Threading.Tasks;
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
    }
}