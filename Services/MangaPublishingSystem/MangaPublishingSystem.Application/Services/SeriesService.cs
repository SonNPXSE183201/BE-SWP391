using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.DTOs.Notifications;
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
        private readonly IBoardVoteRepository _boardVoteRepository;
        private readonly IWalletRepository _walletRepository;

        public SeriesService(
            ISeriesRepository repository, 
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IBoardVoteRepository boardVoteRepository,
            IWalletRepository walletRepository) 
            : base(repository, unitOfWork)
        {
            _seriesRepository = repository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _boardVoteRepository = boardVoteRepository;
            _walletRepository = walletRepository;
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

            // Sử dụng trường Skills để lưu trữ nhãn vắng mặt để tránh thay đổi Check Constraint trên DB vật lý
            user.Skills = onLeave ? "OnLeave" : null;
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

        public async System.Threading.Tasks.Task AcceptFundAsync(int seriesId, int mangakaId)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không phải tác giả của bộ truyện này.");
            }

            if (series.Status != "Fund_Pending")
            {
                throw new ConflictException("Bộ truyện không ở trạng thái chờ nhận vốn cấp phát.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Chuyển trạng thái sang Active
                series.Status = "Active";
                _seriesRepository.Update(series);

                // Cộng số tiền ApprovedProductionBudget vào ví tác giả (Trường SetupFundBalance của Wallet)
                var wallet = await _walletRepository.GetWalletByUserIdAsync(mangakaId);
                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        UserId = mangakaId,
                        SetupFundBalance = 0.00m,
                        WithdrawableBalance = 0.00m,
                        LockedFund = 0.00m,
                        LockedWithdrawable = 0.00m
                    };
                    await _walletRepository.AddAsync(wallet);
                    await _unitOfWork.SaveChangesAsync();
                }

                wallet.SetupFundBalance += series.ApprovedProductionBudget;
                _walletRepository.Update(wallet);
                await _unitOfWork.SaveChangesAsync();

                // Gửi thông báo SignalR cập nhật ví
                await _notificationPublisher.PublishWalletUpdatedAsync(mangakaId, new WalletUpdatedPayload
                {
                    WalletId = wallet.Id,
                    SetupFundBalance = wallet.SetupFundBalance,
                    WithdrawableBalance = wallet.WithdrawableBalance
                });

                // Tạo log thông báo nhận vốn
                var notif = new Notification
                {
                    UserId = mangakaId,
                    Content = $"Xác nhận nhận vốn thành công cho bộ truyện '{series.Title}'. Số tiền {series.ApprovedProductionBudget:N0} VND đã được nạp vào ví ký quỹ của bạn.",
                    Type = "Wallet_Fund_Accepted",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync();

                await _notificationPublisher.PublishNotificationPayloadAsync(mangakaId, new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Nhận vốn thành công",
                    Message = notif.Content,
                    Link = $"/wallet",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                });

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async System.Threading.Tasks.Task VoteSeriesAsync(int seriesId, int boardUserId, bool approved, string comment, decimal recommendedBudget)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "Pending_Approval")
            {
                throw new ConflictException("Chỉ có thể thẩm định bộ truyện đang ở trạng thái Pending_Approval.");
            }

            // Kiểm tra xem đã vote chưa
            var existingVotes = await _boardVoteRepository.FindAsync(v => v.SeriesId == seriesId && v.BoardMemberId == boardUserId);
            if (existingVotes.Any())
            {
                throw new ConflictException("Bạn đã bỏ phiếu thẩm định cho bộ truyện này rồi.");
            }

            var vote = new BoardVote
            {
                SeriesId = seriesId,
                BoardMemberId = boardUserId,
                VoteType = approved ? "Approve" : "Reject",
                RecommendedBudget = approved ? recommendedBudget : 0.00m,
                Comment = comment,
                VoteAt = DateTime.Now
            };

            await _boardVoteRepository.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync();

            if (approved)
            {
                series.Status = "Fund_Pending";
                series.ApprovedProductionBudget = recommendedBudget;
                _seriesRepository.Update(series);

                var notifMangaka = new Notification
                {
                    UserId = series.MangakaId,
                    Content = $"Bộ truyện '{series.Title}' của bạn đã được phê duyệt cấp vốn với ngân sách {recommendedBudget:N0} VND. Vui lòng xác nhận nhận gói vốn để bắt đầu hoạt động.",
                    Type = "Series_Approved",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notifMangaka);
                await _unitOfWork.SaveChangesAsync();

                await _notificationPublisher.PublishNotificationPayloadAsync(series.MangakaId, new NotificationPayload
                {
                    Id = notifMangaka.Id,
                    Title = "Gói vốn của bạn đã được duyệt",
                    Message = notifMangaka.Content,
                    Link = $"/series/{series.Id}",
                    Type = notifMangaka.Type,
                    CreateAt = notifMangaka.CreateAt
                });
            }
            else
            {
                series.Status = "Rejected";
                _seriesRepository.Update(series);

                var notifMangaka = new Notification
                {
                    UserId = series.MangakaId,
                    Content = $"Bộ truyện '{series.Title}' của bạn bị từ chối phê duyệt cấp vốn. Lý do: {comment}",
                    Type = "Series_Rejected",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notifMangaka);
                await _unitOfWork.SaveChangesAsync();

                await _notificationPublisher.PublishNotificationPayloadAsync(series.MangakaId, new NotificationPayload
                {
                    Id = notifMangaka.Id,
                    Title = "Bộ truyện bị từ chối phê duyệt",
                    Message = notifMangaka.Content,
                    Link = $"/series/{series.Id}",
                    Type = notifMangaka.Type,
                    CreateAt = notifMangaka.CreateAt
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}