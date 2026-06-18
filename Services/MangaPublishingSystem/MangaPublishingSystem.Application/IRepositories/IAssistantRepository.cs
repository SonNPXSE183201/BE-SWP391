using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Assistant;

namespace MangaPublishingSystem.Application.IRepositories
{
    public interface IAssistantRepository
    {
        Task<PagedResult<AssistantResponseDto>> GetActiveAssistantsAsync(AssistantFilterDto filter);
    }
}
