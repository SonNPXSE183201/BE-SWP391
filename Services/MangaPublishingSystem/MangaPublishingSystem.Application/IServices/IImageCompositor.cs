using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IImageCompositor
    {
        Task<byte[]> CompositeLayersAsync(string baseLayerUrl, List<CompositeLayerDto> layers);
    }
}
