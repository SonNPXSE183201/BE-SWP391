using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class SeriesService : GenericService<Series>, ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;

        public SeriesService(
            ISeriesRepository repository, 
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher) 
            : base(repository, unitOfWork)
        {
            _seriesRepository = repository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
        }

        public async Task<Series> CreateSeriesAsync(int mangakaId, CreateSeriesDto createDto)
        {
            var series = new Series
            {
                MangakaId = mangakaId,
                Title = createDto.Title,
                Genre = createDto.Genre,
                Synopsis = createDto.Synopsis,
                CoverArtworkUrl = createDto.CoverArtworkUrl,
                EstimatedProductionBudget = createDto.EstimatedProductionBudget,
                ApprovedProductionBudget = 0.00m,
                Status = "Draft"
            };

            await _seriesRepository.AddAsync(series);
            await _unitOfWork.SaveChangesAsync();

            return series;
        }

        public async System.Threading.Tasks.Task SubmitForReviewAsync(int seriesId, int mangakaId, SubmitSeriesReviewDto submitDto)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền gửi duyệt bộ truyện này.");
            }

            if (series.Status != "Draft")
            {
                throw new ConflictException("Bộ truyện đã được gửi thẩm định hoặc đã được duyệt.");
            }

            series.Status = "Pending_Approval";
            _seriesRepository.Update(series);

            // Tự động phân công Biên tập viên (Editor) đầu tiên nếu chưa có
            if (!series.EditorId.HasValue)
            {
                var editors = await _userRepository.FindAsync(u => u.Role.RoleName == "Tantou Editor" && u.Status == UserStatus.Active);
                var editor = editors.FirstOrDefault();
                if (editor != null)
                {
                    series.EditorId = editor.Id;
                    _seriesRepository.Update(series);
                }
            }

            // Gửi thông báo cho Editor
            if (series.EditorId.HasValue)
            {
                var notifEditor = new Notification
                {
                    UserId = series.EditorId.Value,
                    Content = $"Bộ truyện mới '{series.Title}' vừa được gửi yêu cầu phê duyệt bản thảo nháp.",
                    Type = "Series_Pending_Review",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notifEditor);
                await _notificationPublisher.PublishNotificationAsync(series.EditorId.Value, notifEditor.Content, notifEditor.Type);
            }

            // Gửi thông báo cho Mangaka
            var notifMangaka = new Notification
            {
                UserId = mangakaId,
                Content = $"Yêu cầu phê duyệt bộ truyện '{series.Title}' đã được gửi thành công.",
                Type = "Series_Submitted",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notifMangaka);
            await _notificationPublisher.PublishNotificationAsync(mangakaId, notifMangaka.Content, notifMangaka.Type);

            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task SetAbsenceStatusAsync(int mangakaId, bool onLeave)
        {
            var user = await _userRepository.GetByIdAsync(mangakaId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin tác giả.");
            }

            user.IsOnLeave = onLeave;
            _userRepository.Update(user);

            var notif = new Notification
            {
                UserId = mangakaId,
                Content = onLeave ? "Bạn đã bật chế độ nghỉ phép. Bộ đếm thời gian tự động duyệt nhiệm vụ đã được tạm dừng." 
                              : "Bạn đã tắt chế độ nghỉ phép. Hệ thống tiếp tục tính toán hạn duyệt nhiệm vụ.",
                Type = "Absence_Status_Updated",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _notificationPublisher.PublishNotificationAsync(mangakaId, notif.Content, notif.Type);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Series>> GetSeriesByMangakaIdAsync(int mangakaId)
        {
            return await _seriesRepository.FindAsync(s => s.MangakaId == mangakaId);
        }
    }
}