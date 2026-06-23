using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Annotations;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Annotations
{
    [ApiController]
    [Route("api/annotations")]
    public class AnnotationsController : ControllerBase
    {
        private readonly IAnnotationService _annotationService;

        public AnnotationsController(IAnnotationService annotationService)
        {
            _annotationService = annotationService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AnnotationDto>>>> GetAnnotations([FromQuery] int? pageId, [FromQuery] int? taskVersionId)
        {
            var annotations = await _annotationService.GetAnnotationsAsync(pageId, taskVersionId);
            return Ok(ApiResponse<IEnumerable<AnnotationDto>>.Success(annotations, "Tải danh sách chú thích thành công."));
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AnnotationDto>>> GetAnnotationById(int id)
        {
            var annotation = await _annotationService.GetByIdDtoAsync(id);
            return Ok(ApiResponse<AnnotationDto>.Success(annotation, "Tải chi tiết chú thích thành công."));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AnnotationDto>>> CreateAnnotation([FromBody] CreateAnnotationDto createDto)
        {
            var annotation = await _annotationService.CreateAnnotationAsync(CurrentUserId, createDto);
            return Ok(ApiResponse<AnnotationDto>.Success(annotation, "Tạo chú thích thành công."));
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AnnotationDto>>> UpdateAnnotation(int id, [FromBody] UpdateAnnotationDto updateDto)
        {
            var annotation = await _annotationService.UpdateAnnotationAsync(CurrentUserId, id, updateDto);
            return Ok(ApiResponse<AnnotationDto>.Success(annotation, "Cập nhật chú thích thành công."));
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAnnotation(int id)
        {
            await _annotationService.DeleteAnnotationAsync(CurrentUserId, id);
            return Ok(ApiResponse<object>.Success(null, "Xóa chú thích thành công."));
        }
    }
}
