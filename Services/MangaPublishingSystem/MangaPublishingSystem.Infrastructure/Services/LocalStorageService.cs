using System;
using System.IO;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Http;

namespace MangaPublishingSystem.Infrastructure.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalStorageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folderPath = "")
        {
            var folder = string.IsNullOrEmpty(folderPath) ? "uploads" : folderPath.TrimEnd('/');
            var normalizedFolder = folder.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", normalizedFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var destinationStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destinationStream);
            }
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "http://localhost:5010";
            return $"{baseUrl}/{folder}/{uniqueFileName}";
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl)) return Task.FromResult(false);

                var uri = new Uri(fileUrl);
                var localPath = uri.LocalPath.TrimStart('/').Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", localPath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
            }
            catch
            {
                // Bỏ qua lỗi
            }

            return Task.FromResult(false);
        }
    }
}
