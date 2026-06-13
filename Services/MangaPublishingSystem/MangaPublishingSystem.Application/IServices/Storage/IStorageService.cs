using System.IO;
using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices.Storage
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folderName);
    }
}
