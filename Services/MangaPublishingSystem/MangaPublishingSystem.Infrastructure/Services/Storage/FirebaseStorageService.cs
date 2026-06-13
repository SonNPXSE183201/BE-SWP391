using System;
using System.IO;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.IServices.Storage;

namespace MangaPublishingSystem.Infrastructure.Services.Storage
{
    public class FirebaseStorageService : IStorageService
    {
        public Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderName)
        {
            // For future production deployment integration
            // e.g., using FirebaseAdmin or Google.Cloud.Storage.V1 SDK
            throw new NotImplementedException("FirebaseStorageService is not configured for the Development environment. Please set StorageSettings:Provider to 'Local' in appsettings.");
        }
    }
}
