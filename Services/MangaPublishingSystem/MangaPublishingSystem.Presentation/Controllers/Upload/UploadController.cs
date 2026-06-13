using System.IO;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.IServices.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Upload
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IStorageService _storageService;

        public UploadController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> UploadFile(IFormFile file, [FromQuery] string folder = "general")
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.Failure(400, "Vui lòng chọn tệp tin để tải lên."));
            }

            // Ensure folder name is clean (alphanumeric only, to avoid directory traversal attacks)
            var cleanFolder = Path.GetFileName(folder);

            using (var stream = file.OpenReadStream())
            {
                var fileUrl = await _storageService.UploadFileAsync(stream, file.FileName, cleanFolder);
                return Ok(ApiResponse<string>.Success(fileUrl, "Tải lên tệp tin thành công."));
            }
        }
    }
}
