using System;
using System.IO;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MangaPublishingSystem.Application.IServices;

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
        public async Task<ActionResult<ApiResponse<string>>> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.Failure(400, "Vui lòng chọn một file hợp lệ để tải lên."));
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var fileUrl = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType, "uploads");
                    return Ok(ApiResponse<string>.Success(fileUrl, "Tải lên file thành công."));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    ApiResponse<string>.Failure(500, $"Lỗi hệ thống khi tải lên file: {ex.Message}"));
            }
        }
    }
}
