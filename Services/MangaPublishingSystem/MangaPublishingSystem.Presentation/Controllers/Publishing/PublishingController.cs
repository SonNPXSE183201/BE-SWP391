using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Publishing
{
    [ApiController]
    [Route("api/publishing")]
    [Authorize]
    public class PublishingController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public PublishingController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        [HttpGet("schedule")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Chapter>>>> GetSchedule([FromQuery] string month)
        {
            if (string.IsNullOrEmpty(month) || month.Length != 7 || month[4] != '-')
            {
                return BadRequest(ApiResponse<IEnumerable<Chapter>>.Failure(400, "Định dạng tháng không hợp lệ. Vui lòng cung cấp định dạng YYYY-MM."));
            }

            var allChapters = await _chapterService.GetAllAsync();
            var filtered = allChapters.Where(c => 
                c.SubmissionDeadline.HasValue && 
                c.SubmissionDeadline.Value.ToString("yyyy-MM") == month
            );

            return Ok(ApiResponse<IEnumerable<Chapter>>.Success(filtered, $"Lấy lịch xuất bản cho tháng {month} thành công."));
        }

        [HttpPut("schedule/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateDeadline(int id, [FromBody] UpdateDeadlineDto dto)
        {
            await _chapterService.UpdateDeadlineAsync(id, dto.Deadline);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật hạn chót nộp chapter thành công."));
        }

        [HttpPost("schedule/{id}/publish")]
        public async Task<ActionResult<ApiResponse<object>>> PublishChapter(int id)
        {
            await _chapterService.PublishChapterAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Xuất bản chapter thành công."));
        }
    }

    public class UpdateDeadlineDto
    {
        public DateTime Deadline { get; set; }
    }
}
