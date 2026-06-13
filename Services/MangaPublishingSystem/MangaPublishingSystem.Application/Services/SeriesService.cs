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
        private readonly IBoardVoteRepository _boardVoteRepository;

        public SeriesService(
            ISeriesRepository repository, 
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IBoardVoteRepository boardVoteRepository) 
            : base(repository, unitOfWork)
        {
            _seriesRepository = repository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _boardVoteRepository = boardVoteRepository;
        }

        public async Task<SeriesDto> CreateSeriesAsync(int mangakaId, CreateSeriesDto createDto)
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

            return series.ToDto();
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

        public async Task<IEnumerable<SeriesDto>> GetSeriesByMangakaIdAsync(int mangakaId)
        {
            var list = await _seriesRepository.FindAsync(s => s.MangakaId == mangakaId);
            return list.ToDtoList();
        }

        public async System.Threading.Tasks.Task SubmitDraftManuscriptAsync(int seriesId, int mangakaId, string draftManuscriptUrl)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền cập nhật bản thảo cho bộ truyện này.");
            }

            if (series.Status != "Draft")
            {
                throw new ConflictException("Chỉ có thể tải lên bản thảo khi bộ truyện ở trạng thái Draft.");
            }

            series.DraftManuscriptUrl = draftManuscriptUrl;
            _seriesRepository.Update(series);
            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task EvaluateSeriesByEditorAsync(int seriesId, int editorId, EditorEvaluationDto dto)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.EditorId != editorId)
            {
                throw new ForbiddenException("Bạn không phải biên tập viên phụ trách bộ truyện này.");
            }

            if (series.Status != "Pending_Approval")
            {
                throw new ConflictException("Bộ truyện không ở trạng thái chờ thẩm định.");
            }

            series.EditorReport = dto.EvaluationReport;
            series.SuggestedBudget = dto.SuggestedBudget;

            if (dto.IsApproved)
            {
                series.Status = "Pending_Approval";
            }
            else
            {
                series.Status = "Draft";
            }

            _seriesRepository.Update(series);

            var notif = new Notification
            {
                UserId = series.MangakaId,
                Content = dto.IsApproved 
                    ? $"Biên tập viên đã hoàn thành đánh giá bộ truyện '{series.Title}' và trình lên Hội đồng duyệt." 
                    : $"Bộ truyện '{series.Title}' bị từ chối duyệt bản thảo. Lý do: {dto.EvaluationReport}",
                Type = dto.IsApproved ? "Series_Evaluated" : "Series_Rejected",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _notificationPublisher.PublishNotificationAsync(series.MangakaId, notif.Content, notif.Type);

            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task CastBoardVoteAsync(int seriesId, int boardMemberId, BoardVoteDto dto)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "Pending_Approval")
            {
                throw new ConflictException("Bộ truyện không trong trạng thái chờ Hội đồng bình chọn.");
            }

            var existingVotes = await _boardVoteRepository.FindAsync(v => v.SeriesId == seriesId && v.BoardMemberId == boardMemberId);
            if (existingVotes.Any())
            {
                throw new ConflictException("Bạn đã bỏ phiếu cho bộ truyện này rồi.");
            }

            var boardVote = new BoardVote
            {
                SeriesId = seriesId,
                BoardMemberId = boardMemberId,
                VoteType = dto.VoteType,
                RecommendedBudget = dto.RecommendedBudget,
                Comment = dto.Comment,
                VoteAt = DateTime.UtcNow
            };

            await _boardVoteRepository.AddAsync(boardVote);
            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task FinalizeBoardDecisionAsync(int seriesId, BoardDecisionDto dto)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "Pending_Approval")
            {
                throw new ConflictException("Bộ truyện không ở trạng thái chờ phê duyệt.");
            }

            if (dto.IsApproved)
            {
                series.Status = "Board_Approved";
                series.ApprovedProductionBudget = dto.ApprovedProductionBudget;
                series.PublicationSchedule = dto.PublicationSchedule;
            }
            else
            {
                series.Status = "Rejected";
            }

            _seriesRepository.Update(series);

            var notif = new Notification
            {
                UserId = series.MangakaId,
                Content = dto.IsApproved 
                    ? $"Chúc mừng! Bộ truyện '{series.Title}' đã được Hội đồng biên tập phê duyệt với ngân sách {dto.ApprovedProductionBudget:N0} VND." 
                    : $"Rất tiếc, bộ truyện '{series.Title}' đã bị Hội đồng biên tập từ chối phê duyệt.",
                Type = dto.IsApproved ? "Series_Board_Approved" : "Series_Board_Rejected",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _notificationPublisher.PublishNotificationAsync(series.MangakaId, notif.Content, notif.Type);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}