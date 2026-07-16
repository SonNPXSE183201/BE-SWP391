using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Publishing;
using MangaPublishingSystem.Application.IServices;
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
        public async Task<ActionResult<ApiResponse<IEnumerable<PublishingScheduleDto>>>> GetSchedule([FromQuery] string month)
        {
            if (string.IsNullOrEmpty(month) || month.Length != 7 || month[4] != '-')
            {
                return BadRequest(ApiResponse<IEnumerable<PublishingScheduleDto>>.Failure(400, "Định dạng tháng không hợp lệ. Vui lòng dùng YYYY-MM."));
            }

            var schedule = await _chapterService.GetPublishingScheduleAsync(month);

            return Ok(ApiResponse<IEnumerable<PublishingScheduleDto>>.Success(schedule, $"Lấy lịch xuất bản tháng {month} thành công."));
        }

        [HttpPut("schedule/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateDeadline(int id, [FromBody] UpdateDeadlineDto dto)
        {
            await _chapterService.UpdateDeadlineAsync(id, dto.Deadline);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật lịch xuất bản thành công."));
        }

        [HttpPost("schedule/{id}/publish")]
        public async Task<ActionResult<ApiResponse<object>>> PublishChapter(int id)
        {
            await _chapterService.PublishChapterAsync(id);
            return Ok(ApiResponse<object>.Success(null, "Xuất bản chương thành công."));
        }
    }

    public class UpdateDeadlineDto
    {
        public DateTime Deadline { get; set; }
    }
}
