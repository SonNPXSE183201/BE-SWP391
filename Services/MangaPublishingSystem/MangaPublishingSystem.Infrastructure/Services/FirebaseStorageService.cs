using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Storage;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Infrastructure.Models;
using Microsoft.Extensions.Options;

namespace MangaPublishingSystem.Infrastructure.Services
{
    public class FirebaseStorageService : IStorageService
    {
        private readonly FirebaseSettings _settings;

        public FirebaseStorageService(IOptions<FirebaseSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folderPath = "")
        {
            if (string.IsNullOrEmpty(_settings.Bucket))
            {
                throw new InvalidOperationException("Firebase Bucket name chưa được cấu hình.");
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
            var folder = string.IsNullOrEmpty(folderPath) ? "uploads" : folderPath.TrimEnd('/');
            
            var task = new FirebaseStorage(_settings.Bucket)
                .Child(folder)
                .Child(uniqueFileName)
                .PutAsync(fileStream);

            // Chờ quá trình upload hoàn tất và lấy URL tải về
            var downloadUrl = await task;
            return downloadUrl;
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return false;
            if (string.IsNullOrEmpty(_settings.Bucket)) return false;

            try
            {
                // URL có định dạng: https://firebasestorage.googleapis.com/v0/b/{bucket}/o/uploads%2F{fileName}?alt=media
                var uri = new Uri(fileUrl);
                var localPath = uri.LocalPath; // /v0/b/{bucket}/o/uploads%2F{fileName}
                var searchToken = "/o/";
                var index = localPath.IndexOf(searchToken);
                if (index == -1) return false;

                var encodedPath = localPath.Substring(index + searchToken.Length);
                var decodedPath = Uri.UnescapeDataString(encodedPath); // uploads/filename.ext

                var parts = decodedPath.Split('/');
                var storage = new FirebaseStorage(_settings.Bucket);
                FirebaseStorageReference? reference = null;

                foreach (var part in parts)
                {
                    if (reference == null)
                    {
                        reference = storage.Child(part);
                    }
                    else
                    {
                        reference = reference.Child(part);
                    }
                }

                if (reference != null)
                {
                    await reference.DeleteAsync();
                    return true;
                }
            }
            catch
            {
                // Bỏ qua lỗi khi xóa
            }
            return false;
        }
    }
}
