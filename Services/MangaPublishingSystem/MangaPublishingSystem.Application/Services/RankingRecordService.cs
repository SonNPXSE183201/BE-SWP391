using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
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
        private readonly IBoardVotingConfigRepository _boardVotingConfigRepository;
        private readonly IUserRepository _userRepository;

        public RankingRecordService(
            IRankingRecordRepository repository,
            ISeriesRepository seriesRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IBoardVotingConfigRepository boardVotingConfigRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
            _seriesRepository = seriesRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _boardVotingConfigRepository = boardVotingConfigRepository;
            _userRepository = userRepository;
        }

        public async System.Threading.Tasks.Task CreateRankingsAsync(CreateRankingsDto dto, int currentUserId)
        {
            // 0. Kiểm tra quyền: Chỉ Chủ tịch hội đồng biên tập (ChairUserId) hoặc Admin/System Admin mới được phép nhập/chốt
            var configs = await _boardVotingConfigRepository.GetAllAsync();
            var config = configs.FirstOrDefault();
            var users = await _userRepository.FindAsync(u => u.Id == currentUserId, u => u.Role);
            var user = users.FirstOrDefault();

            bool isChair = config != null && config.ChairUserId == currentUserId;
            bool isAdmin = user != null && user.Role != null && (user.Role.RoleName == "Admin" || user.Role.RoleName == "System Admin");

            if (!isChair && !isAdmin)
            {
                throw new ForbiddenException("Chỉ Chủ tịch Hội đồng biên tập hoặc Admin mới có quyền nhập và chốt bảng xếp hạng.");
            }

            if (dto.Records == null || dto.Records.Count == 0)
            {
                return;
            }

            var targetDate = dto.RecordedDate.Date;

            // 1. Lọc trùng SeriesId trong đợt nhập (lấy bản ghi có VoteCount cao nhất nếu nhập lặp)
            var deduplicatedRecords = dto.Records
                .GroupBy(r => r.SeriesId)
                .Select(g => g.OrderByDescending(r => r.VoteCount).First())
                .ToList();

            // 2. Lấy các bản ghi xếp hạng hiện có của cùng ngày chốt số liệu
            var existingOldRecords = (await _repository.FindAsync(r => r.RecordedDate.Date == targetDate)).ToList();

            // 3. Upsert (Cập nhật điểm nếu đã có, Thêm mới nếu chưa có)
            foreach (var rec in deduplicatedRecords)
            {
                var existing = existingOldRecords.FirstOrDefault(r => r.SeriesId == rec.SeriesId);
                if (existing != null)
                {
                    existing.VoteCount += rec.VoteCount;
                    _repository.Update(existing);
                }
                else
                {
                    var newRankingRecord = new RankingRecord
                    {
                        SeriesId = rec.SeriesId,
                        VoteCount = rec.VoteCount,
                        RankPosition = 0,
                        RecordedDate = dto.RecordedDate
                    };
                    await _repository.AddAsync(newRankingRecord);
                }
            }
            await _unitOfWork.SaveChangesAsync();

            // 4. Lấy lại tất cả bản ghi trong ngày đó để Đánh lại vị trí thứ hạng (RankPosition 1..N) theo VoteCount giảm dần
            var allPeriodRecords = (await _repository.FindAsync(r => r.RecordedDate.Date == targetDate))
                .GroupBy(r => r.SeriesId)
                .Select(g => g.OrderByDescending(r => r.VoteCount).First())
                .OrderByDescending(r => r.VoteCount)
                .ToList();

            int totalCount = allPeriodRecords.Count;
            int bottomCount = Math.Max(1, (int)Math.Ceiling(totalCount * 0.2));
            int bottomRankThreshold = totalCount - bottomCount + 1;

            for (int i = 0; i < totalCount; i++)
            {
                var rec = allPeriodRecords[i];
                rec.RankPosition = i + 1;
                _repository.Update(rec);
            }
            await _unitOfWork.SaveChangesAsync();

            // 5. Phát cảnh báo sớm Axing cho nhóm bộ truyện ở vị trí đáy (Bottom Tier)
            for (int i = 0; i < allPeriodRecords.Count; i++)
            {
                var rec = allPeriodRecords[i];
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
            var records = await _repository.FindAsync(r => r.RecordedDate.Date == period.Date, r => r.Series);
            // Đảm bảo dữ liệu trả về duy nhất theo SeriesId phòng trường hợp dữ liệu cũ bị lưu trùng
            return records
                .GroupBy(r => r.SeriesId)
                .Select(g => g.OrderByDescending(r => r.VoteCount).ThenBy(r => r.RankPosition).First())
                .OrderBy(r => r.RankPosition)
                .ToList();
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