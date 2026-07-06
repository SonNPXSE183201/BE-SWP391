using MangaPublishingSystem.Application.DTOs.AI;
using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices.AI
{
    public interface IGeminiClient
    {
        Task<AiTagsResultDto> SuggestTagsAsync(string synopsis);
    }
}
