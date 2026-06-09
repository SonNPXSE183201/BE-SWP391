using System.Collections.Generic;
using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IImageCompositor
    {
        Task<byte[]> CompositeLayersAsync(string baseLayerUrl, List<(string overlayUrl, int zIndex)> layers);
    }
}
