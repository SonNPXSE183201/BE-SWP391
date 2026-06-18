using BuildingBlocks.Web.Responses;

namespace MangaPublishingSystem.Application.DTOs.Assistant
{
    public class AssistantFilterDto : PagedRequest
    {
        public string? SearchTerm { get; set; }
    }
}
