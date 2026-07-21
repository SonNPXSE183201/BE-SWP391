using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Rankings;

namespace MangaPublishingSystem.Application.IServices
{
    public interface IRankingRecordService : IGenericService<RankingRecord>
    {
        System.Threading.Tasks.Task CreateRankingsAsync(CreateRankingsDto dto);
        Task<IEnumerable<RankingRecord>> GetRankingsByPeriodAsync(DateTime period);
        Task<IEnumerable<SeriesRankingSummaryDto>> GetMangakaSeriesRankingsAsync(int mangakaId);
        Task<IEnumerable<RankingHistoryRecordDto>> GetSeriesRankingHistoryAsync(int seriesId);
    }
}