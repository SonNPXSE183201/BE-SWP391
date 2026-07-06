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
                return Ok(ApiResponse<AiSegmentationResultDto>.Success(result, "Phân vùng ảnh thành công."));
            }

            return BadRequest(ApiResponse<AiSegmentationResultDto>.Failure(400, "Lỗi khi gọi mô hình AI phân vùng."));
        }

        [HttpPost("suggest-tags")]
        public async Task<IActionResult> SuggestTags([FromBody] AiTagsRequestDto request)
        {
            var validationResult = await _tagsValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(ApiResponse<AiTagsResultDto>.Failure(400, validationResult.Errors[0].ErrorMessage));
            }

            var result = await _geminiClient.SuggestTagsAsync(request.Synopsis);

            if (result.Success)
            {
                return Ok(ApiResponse<AiTagsResultDto>.Success(result, "Gợi ý thể loại thành công."));
            }

            return BadRequest(ApiResponse<AiTagsResultDto>.Failure(400, result.ErrorMessage));
        }
    }
}
