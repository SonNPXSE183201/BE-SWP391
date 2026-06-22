using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using MangaPublishingSystem.Application.DTOs.Regions;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IRegionService : IGenericService<Region>
    {
        Task<Region> CreateRegionAsync(CreateRegionDto dto);
        Task<IEnumerable<Region>> GetRegionsByPageIdAsync(int pageId);
        Task<Region> UpdateRegionAsync(int id, UpdateRegionDto dto);
        Task DeleteRegionAsync(int id);
    }
}