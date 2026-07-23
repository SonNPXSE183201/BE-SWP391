using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs.Rankings;
using Notification = MangaPublishingSystem.Domain.Entities.Notification;

namespace MangaPublishingSystem.Application.Services
{
    public class RankingRecordService : GenericService<RankingRecord>, IRankingRecordService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;

        public RankingRecordService(
            IRankingRecordRepository repository,
            ISeriesRepository seriesRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
            _seriesRepository = seriesRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
        }

        public async System.Threading.Tasks.Task CreateRankingsAsync(CreateRankingsDto dto)
        {
            var orderedRecords = dto.Records.OrderByDescending(r => r.VoteCount).ToList();
            int totalCount = orderedRecords.Count;
            int bottomCount = Math.Max(1, (int)Math.Ceiling(totalCount * 0.2));
            int bottomRankThreshold = totalCount - bottomCount + 1;

            var createdRecords = new List<RankingRecord>();

            for (int i = 0; i < totalCount; i++)
            {
                var record = orderedRecords[i];
                int rankPos = i + 1;
                var rankingRecord = new RankingRecord
                {
                    SeriesId = record.SeriesId,
                    VoteCount = record.VoteCount,
                    RankPosition = rankPos,
                    RecordedDate = dto.RecordedDate
                };
                await _repository.AddAsync(rankingRecord);
                createdRecords.Add(rankingRecord);
            }
            await _unitOfWork.SaveChangesAsync();

            // Phát cảnh báo sớm Axing cho nhóm bộ truyện ở vị trí đáy (Bottom Tier)
            for (int i = 0; i < createdRecords.Count; i++)
            {
                var rec = createdRecords[i];
                if (rec.RankPosition >= bottomRankThreshold)
                {
                    var series = await _seriesRepository.GetByIdAsync(rec.SeriesId);
                    if (series != null)
                    {
                        var notif = new Notification
                        {
                            UserId = series.MangakaId,
                            Content = $"Cảnh báo Axing: Bộ truyện '{series.Title}' của bạn ở vị trí thứ {rec.RankPosition}/{totalCount} (thuộc nhóm nguy hiểm rớt hạng độc giả kỳ {dto.RecordedDate:yyyy-MM-dd}). Nỗ lực cải thiện nội dung ở các kỳ tới!",
                            Type = "Series_Axing_Warning",
                            IsRead = false
                        };
                        await _notificationRepository.AddAsync(notif);
                        await _notificationPublisher.PublishNotificationAsync(series.MangakaId, notif.Content, notif.Type);
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<RankingRecord>> GetRankingsByPeriodAsync(DateTime period)
        {
            return await _repository.FindAsync(r => r.RecordedDate.Date == period.Date, r => r.Series);
        }

        public async Task<IEnumerable<SeriesRankingSummaryDto>> GetMangakaSeriesRankingsAsync(int mangakaId)
        {
            var seriesList = await _seriesRepository.FindAsync(s => s.MangakaId == mangakaId);
            var results = new List<SeriesRankingSummaryDto>();

            foreach (var series in seriesList)
            {
                var records = (await _repository.FindAsync(r => r.SeriesId == series.Id)).ToList();
                var latestRecord = records.OrderByDescending(r => r.RecordedDate).ThenBy(r => r.RankPosition).FirstOrDefault();

                if (latestRecord != null)
                {
                    var samePeriodRecords = (await _repository.FindAsync(r => r.RecordedDate.Date == latestRecord.RecordedDate.Date)).ToList();
                    int totalInPeriod = samePeriodRecords.Count;
                    int bottomCount = Math.Max(1, (int)Math.Ceiling(totalInPeriod * 0.2));
                    bool isBottom = latestRecord.RankPosition > (totalInPeriod - bottomCount);

                    results.Add(new SeriesRankingSummaryDto
                    {
                        SeriesId = series.Id,
                        SeriesTitle = series.Title,
                        CurrentRank = latestRecord.RankPosition,
                        TotalVotes = latestRecord.VoteCount,
                        RecordedDate = latestRecord.RecordedDate,
                        IsBottomTier = isBottom
                    });
                }
                else
                {
                    results.Add(new SeriesRankingSummaryDto
                    {
                        SeriesId = series.Id,
                        SeriesTitle = series.Title,
                        CurrentRank = 0,
                        TotalVotes = 0,
                        RecordedDate = DateTime.MinValue,
                        IsBottomTier = false
                    });
                }
            }

            return results;
        }

        public async Task<IEnumerable<RankingHistoryRecordDto>> GetSeriesRankingHistoryAsync(int seriesId)
        {
            var records = await _repository.FindAsync(r => r.SeriesId == seriesId);
            return records.OrderByDescending(r => r.RecordedDate)
                .Select(r => new RankingHistoryRecordDto
                {
                    RecordedDate = r.RecordedDate,
                    RankPosition = r.RankPosition,
                    VoteCount = r.VoteCount
                })
                .ToList();
        }
    }
}