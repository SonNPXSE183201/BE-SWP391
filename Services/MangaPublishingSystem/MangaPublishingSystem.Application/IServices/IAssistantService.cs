using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Assistant;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IAssistantService
    {
        Task<PagedResult<AssistantResponseDto>> GetActiveAssistantsAsync(AssistantFilterDto filter);
    }
}
