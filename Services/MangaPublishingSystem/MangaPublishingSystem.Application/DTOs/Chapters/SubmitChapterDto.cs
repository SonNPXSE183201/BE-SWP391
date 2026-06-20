using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MangaPublishingSystem.Application.DTOs.Chapters
{
    public class SubmitChapterDto
    {
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = null!;
        public List<IFormFile> Pages { get; set; } = new List<IFormFile>();
    }
}
