using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.AI;
using MangaPublishingSystem.Application.IServices.AI;

namespace MangaPublishingSystem.Presentation.Controllers.AI
{
    [ApiController]
    [Route("api/ai")]
    public class AiIntegrationController : ControllerBase
    {
        private readonly IAiVisionClient _aiVisionClient;
        private readonly IGeminiClient _geminiClient;
        private readonly FluentValidation.IValidator<AiTagsRequestDto> _tagsValidator;

        public AiIntegrationController(IAiVisionClient aiVisionClient, IGeminiClient geminiClient, FluentValidation.IValidator<AiTagsRequestDto> tagsValidator)
        {
            _aiVisionClient = aiVisionClient;
            _geminiClient = geminiClient;
            _tagsValidator = tagsValidator;
        }

        [HttpPost("segment")]
        public async Task<IActionResult> TestSegmentation([FromBody] string imageUrl)
        {
            var result = await _aiVisionClient.SegmentMangaPanelsAsync(imageUrl);
            
            if (result.Success)
            {
                // FE-Friendly: Chỉ trả về mảng panels, bỏ qua trường success thừa thãi bên trong Data
                return Ok(ApiResponse<object>.Success(new { panels = result.Panels }, "Phân vùng ảnh thành công."));
            }

            return BadRequest(ApiResponse<object>.Failure(400, "Lỗi khi gọi mô hình AI phân vùng."));
        }

        [HttpPost("segment/visualize")]
        public async Task<IActionResult> TestSegmentationDraw([FromBody] string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest(ApiResponse<object>.Failure(400, "Image URL is required."));
            }

            var result = await _aiVisionClient.SegmentAndDrawMangaAsync(imageUrl);
            
            if (result.Success)
            {
                // FE-Friendly: Chỉ trả về URL ảnh đã vẽ
                return Ok(ApiResponse<object>.Success(new { drawnImageUrl = result.ColorizedImageUrl }, "Vẽ khung nhận diện thành công."));
            }

            return BadRequest(ApiResponse<object>.Failure(400, result.ErrorMessage ?? "Lỗi khi vẽ khung AI phân vùng."));
        }

        [HttpPost("suggest-tags")]
        public async Task<IActionResult> SuggestTags([FromBody] AiTagsRequestDto request)
        {
            var validationResult = await _tagsValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<object>.Failure(400, validationResult.Errors[0].ErrorMessage));
            }

            var result = await _geminiClient.SuggestTagsAsync(request.Synopsis);

            if (result.Success)
            {
                // FE-Friendly: Trả trực tiếp mảng chuỗi Tags
                return Ok(ApiResponse<List<string>>.Success(result.Tags, "Gợi ý thể loại thành công."));
            }

            return BadRequest(ApiResponse<object>.Failure(400, result.ErrorMessage));
        }

        [HttpPost("colorize")]
        public async Task<IActionResult> TestColorization([FromBody] string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return BadRequest(ApiResponse<object>.Failure(400, "Image URL is required."));
            }

            var result = await _aiVisionClient.ColorizeMangaAsync(imageUrl);
            
            if (result.Success)
            {
                // FE-Friendly: Chỉ trả về URL ảnh
                return Ok(ApiResponse<object>.Success(new { colorizedImageUrl = result.ColorizedImageUrl }, "Tô màu ảnh thành công."));
            }

            return BadRequest(ApiResponse<object>.Failure(400, result.ErrorMessage ?? "Lỗi khi gọi mô hình AI tô màu."));
        }
    }
}
