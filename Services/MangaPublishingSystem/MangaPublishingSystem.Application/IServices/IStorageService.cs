using System.IO;
using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IStorageService
    {
        /// <summary>
        /// Tải file lên bộ lưu trữ và trả về URL tuyệt đối để truy cập công cộng.
        /// </summary>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folderPath = "");

        /// <summary>
        /// Xóa file khỏi bộ lưu trữ bằng đường dẫn URL hoặc tên file.
        /// </summary>
        Task<bool> DeleteFileAsync(string fileUrl);
    }
}
