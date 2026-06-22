using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Application.DTOs.Series;
using MangaPublishingSystem.Application.DTOs.Chapters;
using MangaPublishingSystem.Application.DTOs.Reviews;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using Microsoft.AspNetCore.Hosting;

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
        private readonly IChapterRepository _chapterRepository;
        private readonly IPageRepository _pageRepository;
        private readonly IWebHostEnvironment _env;

        public SeriesService(
            ISeriesRepository repository, 
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IBoardVoteRepository boardVoteRepository,
            IWalletRepository walletRepository,
            IChapterRepository chapterRepository,
            IPageRepository pageRepository,
            IWebHostEnvironment env) 
            : base(repository, unitOfWork)
        {
            _seriesRepository = repository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _boardVoteRepository = boardVoteRepository;
            _walletRepository = walletRepository;
            _chapterRepository = chapterRepository;
            _pageRepository = pageRepository;
            _env = env;
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

            // Tự động phân công Biên tập viên (Editor) từ thông tin Mangaka
            if (!series.EditorId.HasValue)
            {
                var mangaka = await _userRepository.GetByIdAsync(mangakaId);
                if (mangaka != null && mangaka.AssignedEditorId.HasValue)
                {
                    series.EditorId = mangaka.AssignedEditorId.Value;
                }
                else
                {
                    var editors = await _userRepository.FindAsync(u => u.Role.RoleName == "Tantou Editor" && u.Status == UserStatus.Active);
                    var editor = editors.FirstOrDefault();
                    if (editor != null)
                    {
                        series.EditorId = editor.Id;
                    }
                }

                if (series.EditorId.HasValue)
                {
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

            if (series.Status != "Pending_Approval" && series.Status != "Pending_Board_Vote" && series.Status != "Fund_Pending" && series.Status != "Rejected")
            {
                throw new ConflictException("Chỉ có thể thẩm định bộ truyện đang ở trạng thái Pending_Approval, Pending_Board_Vote, Fund_Pending hoặc Rejected.");
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

            // Lấy danh sách thành viên hội đồng hoạt động
            var activeBoardMembers = await _userRepository.FindAsync(u => u.RoleId == 3 && u.Status == UserStatus.Active);
            int N = activeBoardMembers.Count();
            if (N == 0) N = 1; // Tránh lỗi chia cho 0

            int approveThreshold = (N / 2) + 1;
            int rejectThreshold = N - approveThreshold + 1;

            var allVotes = await _boardVoteRepository.FindAsync(v => v.SeriesId == seriesId);
            int approveCount = allVotes.Count(v => v.VoteType == "Approve");
            int rejectCount = allVotes.Count(v => v.VoteType == "Reject");

            if (approveCount >= approveThreshold)
            {
                bool wasAlreadyApproved = (series.Status == "Fund_Pending");
                
                var approveVotes = allVotes.Where(v => v.VoteType == "Approve").ToList();
                decimal averageBudget = approveVotes.Any() ? approveVotes.Average(v => v.RecommendedBudget) : recommendedBudget;

                series.Status = "Fund_Pending";
                series.ApprovedProductionBudget = averageBudget;
                _seriesRepository.Update(series);

                if (!wasAlreadyApproved)
                {
                    var notifMangaka = new Notification
                    {
                        UserId = series.MangakaId,
                        Content = $"Bộ truyện '{series.Title}' của bạn đã được phê duyệt cấp vốn với ngân sách {averageBudget:N0} VND. Vui lòng xác nhận nhận gói vốn để bắt đầu hoạt động.",
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
            }
            else if (rejectCount >= rejectThreshold)
            {
                bool wasAlreadyRejected = (series.Status == "Rejected");
                series.Status = "Rejected";
                _seriesRepository.Update(series);

                if (!wasAlreadyRejected)
                {
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
            }
            else
            {
                series.Status = "Pending_Board_Vote";
                _seriesRepository.Update(series);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Chapter> SubmitChapterAsync(int seriesId, int mangakaId, SubmitChapterDto dto)
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

            if (series.Status != "Active" && series.Status != "Fund_Pending")
            {
                throw new ConflictException("Bộ truyện chưa được kích hoạt, không thể tạo chapter mới.");
            }

            var chapter = new Chapter
            {
                SeriesId = seriesId,
                ChapterNumber = dto.ChapterNumber,
                Title = dto.Title,
                ValidPageCount = dto.Pages?.Count ?? 0,
                AppliedGenkouryoPrice = 0,
                Status = "Pending_Review"
            };

            await _chapterRepository.AddAsync(chapter);
            await _unitOfWork.SaveChangesAsync();

            // Lưu từng trang ảnh vào wwwroot/uploads
            if (dto.Pages != null && dto.Pages.Count > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                int pageNumber = 1;
                foreach (var file in dto.Pages)
                {
                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var imageUrl = $"/uploads/{fileName}";
                    var page = new Page
                    {
                        ChapterId = chapter.Id,
                        PageNumber = pageNumber,
                        RawImageUrl = imageUrl,
                        BaseLayerUrl = imageUrl,
                        Status = "Pending",
                        IsApproved = false
                    };

                    await _pageRepository.AddAsync(page);
                    pageNumber++;
                }

                await _unitOfWork.SaveChangesAsync();
            }

            // Gửi thông báo cho Editor nếu có
            if (series.EditorId.HasValue)
            {
                var notif = new Notification
                {
                    UserId = series.EditorId.Value,
                    Content = $"Tác giả đã nộp chapter mới '{chapter.Title}' (Chapter {chapter.ChapterNumber}) cho bộ truyện '{series.Title}'.",
                    Type = "Chapter_Submitted",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync();
                await _notificationPublisher.PublishNotificationAsync(series.EditorId.Value, notif.Content, notif.Type);
            }

            return chapter;
        }

        public async Task<SeriesReviewDto> GetSeriesReviewAsync(int seriesId)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            var chapters = await _chapterRepository.FindAsync(c => c.SeriesId == seriesId);
            var chapterList = chapters.ToList();

            var mangaka = await _userRepository.GetByIdAsync(series.MangakaId);
            var editor = series.EditorId.HasValue ? await _userRepository.GetByIdAsync(series.EditorId.Value) : null;

            return new SeriesReviewDto
            {
                Id = series.Id,
                Title = series.Title,
                Genre = series.Genre,
                Synopsis = series.Synopsis,
                CoverArtworkUrl = series.CoverArtworkUrl,
                EstimatedProductionBudget = series.EstimatedProductionBudget,
                ApprovedProductionBudget = series.ApprovedProductionBudget,
                Status = series.Status,
                MangakaId = series.MangakaId,
                MangakaName = mangaka?.FullName,
                EditorId = series.EditorId,
                EditorName = editor?.FullName,
                ChapterCount = chapterList.Count,
                Chapters = chapterList.Select(c => new ChapterSummaryDto
                {
                    Id = c.Id,
                    ChapterNumber = c.ChapterNumber,
                    Title = c.Title,
                    Status = c.Status,
                    PageCount = c.ValidPageCount
                }).ToList(),
                CreateAt = series.CreateAt,
                UpdateAt = series.UpdateAt
            };
        }

        public async System.Threading.Tasks.Task SubmitSeriesToBoardAsync(int seriesId, int editorId, SubmitToBoardDto dto)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "Pending_Approval")
            {
                throw new ConflictException("Chỉ có thể gửi duyệt hội đồng khi bộ truyện ở trạng thái Pending_Approval.");
            }

            series.Status = "Pending_Board_Vote";
            _seriesRepository.Update(series);

            // Gửi thông báo cho Mangaka
            var notifMangaka = new Notification
            {
                UserId = series.MangakaId,
                Content = $"Bộ truyện '{series.Title}' đã được biên tập viên chuyển lên hội đồng thẩm định. {(string.IsNullOrEmpty(dto.Notes) ? "" : $"Ghi chú: {dto.Notes}")}",
                Type = "Series_Submitted_To_Board",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notifMangaka);
            await _notificationPublisher.PublishNotificationAsync(series.MangakaId, notifMangaka.Content, notifMangaka.Type);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Series>> GetPendingBoardVoteSeriesAsync()
        {
            return await _seriesRepository.FindAsync(s => s.Status == "Pending_Board_Vote" || s.Status == "Pending_Approval");
        }
    }
}