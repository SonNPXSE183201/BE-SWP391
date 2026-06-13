using System;
using System.IO;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.IServices.Storage;

namespace MangaPublishingSystem.Infrastructure.Services.Storage
{
    public class LocalStorageService : IStorageService
    {
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderName)
        {
            var baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadDir = Path.Combine(baseDirectory, "uploads", folderName);

            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var ws = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(ws);
            }

            // Return relative URL for static file serving
            return $"/uploads/{folderName}/{uniqueFileName}";
        }
    }
}
