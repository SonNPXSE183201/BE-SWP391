using System;
using System.Collections.Generic;
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

        public RankingsController(IRankingRecordService rankingRecordService)
        {
            _rankingRecordService = rankingRecordService;
        }

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
    }
}
