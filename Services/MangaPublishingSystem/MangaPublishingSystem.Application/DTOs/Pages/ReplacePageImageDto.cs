using Microsoft.AspNetCore.Http;

namespace MangaPublishingSystem.Application.DTOs.Pages
{
    public class ReplacePageImageDto
    {
        public IFormFile File { get; set; } = null!;
    }
}
