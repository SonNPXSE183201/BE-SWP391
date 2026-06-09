using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Series
{
    [ApiController]
    [Route("api/series")]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize(Roles = "Mangaka")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SeriesDto>>> Create([FromBody] CreateSeriesDto createDto)
        {
            int mangakaId = CurrentUserId;
            var series = await _seriesService.CreateSeriesAsync(mangakaId, createDto);
            var result = MapToSeriesDto(series);
            return Ok(ApiResponse<SeriesDto>.Success(result, "Tạo hồ sơ bộ truyện mới thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/submit-review")]
        public async Task<ActionResult<ApiResponse<object>>> SubmitForReview([FromRoute] int id, [FromBody] SubmitSeriesReviewDto submitDto)
        {
            int mangakaId = CurrentUserId;
            await _seriesService.SubmitForReviewAsync(id, mangakaId, submitDto);
            return Ok(ApiResponse<object>.Success(null, "Gửi hồ sơ bộ truyện duyệt thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("absence")]
        public async Task<ActionResult<ApiResponse<object>>> SetAbsenceStatus([FromQuery] bool onLeave)
        {
            int mangakaId = CurrentUserId;
            await _seriesService.SetAbsenceStatusAsync(mangakaId, onLeave);
            var message = onLeave ? "Bật trạng thái nghỉ phép thành công." : "Tắt trạng thái nghỉ phép thành công.";
            return Ok(ApiResponse<object>.Success(null, message));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpGet("my-list")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeriesDto>>>> GetMySeries()
        {
            int mangakaId = CurrentUserId;
            var seriesList = await _seriesService.GetSeriesByMangakaIdAsync(mangakaId);
            var result = seriesList.Select(MapToSeriesDto).ToList();
            return Ok(ApiResponse<IEnumerable<SeriesDto>>.Success(result, "Lấy danh sách bộ truyện thành công."));
        }

        private static SeriesDto MapToSeriesDto(MangaPublishingSystem.Domain.Entities.Series series)
        {
            return new SeriesDto
            {
                Id = series.Id,
                MangakaId = series.MangakaId,
                EditorId = series.EditorId,
                Title = series.Title,
                Genre = series.Genre,
                Synopsis = series.Synopsis,
                CoverArtworkUrl = series.CoverArtworkUrl,
                EstimatedProductionBudget = series.EstimatedProductionBudget,
                ApprovedProductionBudget = series.ApprovedProductionBudget,
                PublicationSchedule = series.PublicationSchedule,
                Status = series.Status,
                ResourceFolderUrl = series.ResourceFolderUrl,
                MangakaName = series.Mangaka?.FullName,
                EditorName = series.Editor?.FullName,
                CreateAt = series.CreateAt,
                UpdateAt = series.UpdateAt
            };
        }
    }
}
