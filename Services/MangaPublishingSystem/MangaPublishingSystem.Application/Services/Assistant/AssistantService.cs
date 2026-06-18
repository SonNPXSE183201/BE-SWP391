using System;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Assistant;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services.Assistant
{
    public class AssistantService : IAssistantService
    {
        private readonly IAssistantRepository _assistantRepository;

        public AssistantService(IAssistantRepository assistantRepository)
        {
            _assistantRepository = assistantRepository;
        }

        public async Task<PagedResult<AssistantResponseDto>> GetActiveAssistantsAsync(AssistantFilterDto filter)
        {
            // Normalize page settings if needed, though PagedRequest handles some of it
            return await _assistantRepository.GetActiveAssistantsAsync(filter);
        }
    }
}
