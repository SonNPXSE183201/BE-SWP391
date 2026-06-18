using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Regions;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IRegionService : IGenericService<Region>
    {
        Task<Region> CreateRegionAsync(CreateRegionDto dto);
    }
}