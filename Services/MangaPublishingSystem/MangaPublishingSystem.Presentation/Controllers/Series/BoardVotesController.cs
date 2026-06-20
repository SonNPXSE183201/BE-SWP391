using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Series
{
    [ApiController]
    [Route("api/votes")]
    [Authorize(Roles = "Editorial Board")]
    public class BoardVotesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public BoardVotesController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        [HttpGet("pending")]
        public async Task<ActionResult<ApiResponse<IEnumerable<SeriesDto>>>> GetPendingSeries()
        {
            var seriesList = await _seriesService.GetPendingBoardVoteSeriesAsync();
            var result = seriesList.Select(s => new SeriesDto
            {
                Id = s.Id,
                MangakaId = s.MangakaId,
                EditorId = s.EditorId,
                Title = s.Title,
                Genre = s.Genre,
                Synopsis = s.Synopsis,
                CoverArtworkUrl = s.CoverArtworkUrl,
                EstimatedProductionBudget = s.EstimatedProductionBudget,
                ApprovedProductionBudget = s.ApprovedProductionBudget,
                PublicationSchedule = s.PublicationSchedule,
                Status = s.Status,
                ResourceFolderUrl = s.ResourceFolderUrl,
                MangakaName = s.Mangaka?.FullName,
                EditorName = s.Editor?.FullName,
                CreateAt = s.CreateAt,
                UpdateAt = s.UpdateAt
            }).ToList();
            return Ok(ApiResponse<IEnumerable<SeriesDto>>.Success(result, "Lấy danh sách bộ truyện chờ thẩm định thành công."));
        }
    }
}
