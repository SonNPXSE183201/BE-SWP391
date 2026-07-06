using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.DTOs.Chapters;
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
        private readonly IChapterService _chapterService;

        public SeriesController(ISeriesService seriesService, IChapterService chapterService)
        {
            _seriesService = seriesService;
            _chapterService = chapterService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeriesDto>>>> GetAllSeries([FromQuery] string? status)
        {
            var series = await _seriesService.GetAllAsync();
            if (!string.IsNullOrEmpty(status))
            {
                series = series.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            var result = series.Select(MapToSeriesDto).ToList();
            return Ok(ApiResponse<IEnumerable<SeriesDto>>.Success(result, "Lấy danh sách tất cả bộ truyện thành công."));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SeriesDto>>> GetSeriesById([FromRoute] int id)
        {
            var series = await _seriesService.GetByIdAsync(id);
            if (series == null)
            {
                return NotFound(ApiResponse<SeriesDto>.Failure(404, "Không tìm thấy bộ truyện này."));
            }

            var result = MapToSeriesDto(series);
            result.HasContract = await _seriesService.HasContractAsync(series.Id);
            return Ok(ApiResponse<SeriesDto>.Success(result, "Lấy thông tin chi tiết bộ truyện thành công."));
        }

        [HttpGet("{id}/chapters")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ChapterDto>>>> GetChaptersBySeriesId([FromRoute] int id)
        {
            var chapters = await _chapterService.GetChaptersBySeriesIdAsync(id);
            var result = chapters.Select(c => new ChapterDto
            {
                Id = c.Id,
                SeriesId = c.SeriesId,
                ChapterNumber = c.ChapterNumber,
                Title = c.Title,
                ValidPageCount = c.ValidPageCount,
                AppliedGenkouryoPrice = c.AppliedGenkouryoPrice,
                SubmissionDeadline = c.SubmissionDeadline,
                QcChecklistData = c.QcChecklistData,
                Status = c.Status,
                CreateAt = c.CreateAt,
                UpdateAt = c.UpdateAt
            }).ToList();

            return Ok(ApiResponse<IEnumerable<ChapterDto>>.Success(result, "Lấy danh sách chương truyện thành công."));
        }

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
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SeriesDto>>> Update([FromRoute] int id, [FromBody] CreateSeriesDto updateDto)
        {
            await _seriesService.UpdateSeriesAsync(id, CurrentUserId, updateDto);
            var series = await _seriesService.GetByIdAsync(id);
            var result = MapToSeriesDto(series!);
            return Ok(ApiResponse<SeriesDto>.Success(result, "Cập nhật hồ sơ bộ truyện thành công."));
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

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/accept-fund")]
        public async Task<ActionResult<ApiResponse<object>>> AcceptFund([FromRoute] int id)
        {
            await _seriesService.AcceptFundAsync(id, CurrentUserId);
            return Ok(ApiResponse<object>.Success(null, "Xác nhận nhận vốn và kích hoạt bộ truyện thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/decline-fund")]
        public async Task<ActionResult<ApiResponse<object>>> DeclineFund([FromRoute] int id)
        {
            await _seriesService.DeclineFundAsync(id, CurrentUserId);
            return Ok(ApiResponse<object>.Success(null, "Đã từ chối vốn cấp phát. Bộ truyện quay về bản nháp."));
        }

        [Authorize(Roles = "Editorial Board")]
        [HttpPost("{id}/vote")]
        public async Task<ActionResult<ApiResponse<object>>> VoteSeries([FromRoute] int id, [FromBody] VoteSeriesRequestDto dto)
        {
            var voteChoice = !string.IsNullOrWhiteSpace(dto.VoteChoice)
                ? dto.VoteChoice
                : (dto.Approved ? "Approve" : "Reject");
            await _seriesService.VoteSeriesAsync(id, CurrentUserId, voteChoice, dto.Comment, dto.RecommendedBudget);
            return Ok(ApiResponse<object>.Success(null, "Bỏ phiếu thẩm định bộ truyện thành công."));
        }

        [Authorize(Roles = "Mangaka")]
        [HttpPost("{id}/chapters")]
        public async Task<ActionResult<ApiResponse<ChapterDto>>> SubmitChapter([FromRoute] int id, [FromForm] Application.DTOs.Chapters.SubmitChapterDto dto)
        {
            int mangakaId = CurrentUserId;
            var chapter = await _seriesService.SubmitChapterAsync(id, mangakaId, dto);
            var result = new ChapterDto
            {
                Id = chapter.Id,
                SeriesId = chapter.SeriesId,
                ChapterNumber = chapter.ChapterNumber,
                Title = chapter.Title,
                ValidPageCount = chapter.ValidPageCount,
                AppliedGenkouryoPrice = chapter.AppliedGenkouryoPrice,
                Status = chapter.Status,
                CreateAt = chapter.CreateAt,
                UpdateAt = chapter.UpdateAt
            };
            return Ok(ApiResponse<ChapterDto>.Success(result, "Tạo chapter nháp và tải lên trang truyện thành công."));
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
                EditorRecommendedBudget = series.EditorRecommendedBudget,
                ApprovedProductionBudget = series.ApprovedProductionBudget,
                PublicationSchedule = series.PublicationSchedule,
                Status = series.Status,
                ResourceFolderUrl = series.ResourceFolderUrl,
                MangakaName = series.Mangaka?.FullName,
                EditorName = series.Editor?.FullName,
                EditorNote = series.EditorNote,
                MangakaSubmissionNote = series.MangakaSubmissionNote,
                CreateAt = series.CreateAt,
                UpdateAt = series.UpdateAt
            };
        }
    }
}

