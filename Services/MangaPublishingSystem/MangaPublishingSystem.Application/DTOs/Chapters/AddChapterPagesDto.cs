using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MangaPublishingSystem.Application.DTOs.Chapters
{
    public class AddChapterPagesDto
    {
        public List<IFormFile> Pages { get; set; } = new List<IFormFile>();
    }
}
