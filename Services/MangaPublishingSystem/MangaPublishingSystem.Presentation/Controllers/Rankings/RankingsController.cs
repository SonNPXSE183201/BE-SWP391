using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Application.DTOs.Rankings;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaPublishingSystem.Presentation.Controllers.Rankings
{
    [ApiController]
    [Route("api/rankings")]
    [Authorize]
    public class RankingsController : ControllerBase
    {
        private readonly IRankingRecordService _rankingRecordService;
        private readonly ISeriesService _seriesService;

        public RankingsController(IRankingRecordService rankingRecordService, ISeriesService seriesService)
        {
            _rankingRecordService = rankingRecordService;
            _seriesService = seriesService;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CreateRankings([FromBody] CreateRankingsDto dto)
        {
            await _rankingRecordService.CreateRankingsAsync(dto);
            return Ok(ApiResponse<object>.Success(null, "Tạo bảng xếp hạng thành công."));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<RankingRecord>>>> GetRankings([FromQuery] string period)
        {
            if (!DateTime.TryParse(period, out DateTime parsedDate))
            {
                return BadRequest(ApiResponse<IEnumerable<RankingRecord>>.Failure(400, "Định dạng kỳ ngày không hợp lệ. Vui lòng cung cấp định dạng YYYY-MM-DD."));
            }

            var rankings = await _rankingRecordService.GetRankingsByPeriodAsync(parsedDate);
            return Ok(ApiResponse<IEnumerable<RankingRecord>>.Success(rankings, $"Lấy bảng xếp hạng cho ngày {period} thành công."));
        }

        [Authorize(Roles = "Editorial Board")]
        [HttpPost("/api/ranking/votes")]
        public async Task<ActionResult<ApiResponse<object>>> VoteRanking([FromBody] VoteRankingRequestDto dto)
        {
            await _seriesService.VoteRankingAsync(dto.SeriesId, CurrentUserId, dto.VoteType, dto.Comment);
            return Ok(ApiResponse<object>.Success(null, "Bình chọn bảng xếp hạng thành công."));
        }
    }
}

