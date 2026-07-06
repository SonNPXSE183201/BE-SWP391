using System.Threading;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.AI;

namespace MangaPublishingSystem.Application.IServices.AI
{
    public interface IAiVisionClient
    {
        Task<AiSegmentationResultDto> SegmentMangaPanelsAsync(string imageUrl, CancellationToken cancellationToken = default);
    }
}
