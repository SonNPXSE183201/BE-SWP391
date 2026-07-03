using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Assistant;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IAssistantService
    {
        Task<PagedResult<AssistantResponseDto>> GetActiveAssistantsAsync(AssistantFilterDto filter);
        Task<IEnumerable<AssistantInviteDto>> GetMyInvitesAsync(int assistantId);
        Task<AssistantProfileDto> GetProfileAsync(int assistantId);
        Task<AssistantProfileDto> UpdateProfileAsync(int assistantId, UpdateAssistantProfileDto dto);
        Task<AssistantPortfolioStatsDto> GetPortfolioStatsAsync(int assistantId);
        Task<IEnumerable<PortfolioSampleDto>> GetPortfolioSamplesAsync(int assistantId);
        Task<PortfolioSampleDto> CreatePortfolioSampleAsync(int assistantId, CreatePortfolioSampleDto dto);
        Task<PortfolioSampleDto> UpdatePortfolioSampleAsync(int assistantId, int sampleId, UpdatePortfolioSampleDto dto);
        Task DeletePortfolioSampleAsync(int assistantId, int sampleId);
    }
}
