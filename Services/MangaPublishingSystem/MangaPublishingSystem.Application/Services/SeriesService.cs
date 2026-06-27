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
using MangaPublishingSystem.Application.Options;
using Microsoft.Extensions.Options;

namespace MangaPublishingSystem.Application.Services
{
    public class SeriesService : GenericService<Series>, ISeriesService
    {
        private const string RoleNameTantouEditor = "Tantou Editor";

        private readonly ISeriesRepository _seriesRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IBoardVoteRepository _boardVoteRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IPageRepository _pageRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly IWalletService _walletService;
        private readonly IStorageService _storageService;
        private readonly BoardVoteSettings _boardVoteSettings;

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
            ITasksRepository tasksRepository,
            IRegionRepository regionRepository,
            IWalletService walletService,
            IStorageService storageService,
            IOptions<BoardVoteSettings> boardVoteSettings) 
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
            _tasksRepository = tasksRepository;
            _regionRepository = regionRepository;
            _walletService = walletService;
            _storageService = storageService;
            _boardVoteSettings = boardVoteSettings.Value;
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
                ResourceFolderUrl = createDto.ResourceFolderUrl,
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

            var mangaka = await _userRepository.GetByIdAsync(mangakaId);
            if (mangaka == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin tác giả.");
            }

            if (!mangaka.AssignedEditorId.HasValue)
            {
                throw new BadRequestException(
                    "Tác giả chưa được Admin gán Biên tập viên phụ trách. Vui lòng liên hệ quản trị viên.");
            }

            if (string.IsNullOrWhiteSpace(series.ResourceFolderUrl))
            {
                throw new BadRequestException(
                    "Vui lòng upload bản phác thảo (Name) trước khi gửi xét duyệt.");
            }

            var assignedEditor = await _userRepository.GetByIdWithDetailsAsync(mangaka.AssignedEditorId.Value);
            if (assignedEditor == null
                || assignedEditor.Role?.RoleName != RoleNameTantouEditor
                || assignedEditor.Status != UserStatus.Active)
            {
                throw new BadRequestException("Biên tập viên phụ trách không tồn tại hoặc không hoạt động.");
            }

            series.Status = "Pending_Approval";
            series.EditorId = mangaka.AssignedEditorId.Value;
            _seriesRepository.Update(series);
            await _unitOfWork.SaveChangesAsync();

            var editorNoteSuffix = string.IsNullOrWhiteSpace(submitDto.SubmissionNotes)
                ? string.Empty
                : $" Ghi chú: {submitDto.SubmissionNotes.Trim()}";

            // Gửi thông báo realtime cho Editor phụ trách (Phương án 3 — A04)
            var notifEditor = new Notification
            {
                UserId = series.EditorId.Value,
                Content = $"Bộ truyện mới '{series.Title}' vừa được gửi yêu cầu phê duyệt bản thảo nháp.{editorNoteSuffix}",
                Type = "Series_Pending_Review",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notifEditor);
            await _unitOfWork.SaveChangesAsync();

            var notifEditorPayload = new NotificationPayload
            {
                Id = notifEditor.Id,
                Title = "Series mới chờ duyệt",
                Message = notifEditor.Content,
                Link = $"/editor/review/{series.Id}",
                Type = "Series_Pending_Review",
                CreateAt = notifEditor.CreateAt
            };
            await _notificationPublisher.PublishNotificationPayloadAsync(series.EditorId.Value, notifEditorPayload);

            // Gửi thông báo cho Mangaka
            var notifMangaka = new Notification
            {
                UserId = mangakaId,
                Content = $"Yêu cầu phê duyệt bộ truyện '{series.Title}' đã được gửi thành công tới biên tập viên {assignedEditor.FullName}.",
                Type = "Series_Submitted",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notifMangaka);
            await _unitOfWork.SaveChangesAsync();

            var notifMangakaPayload = new NotificationPayload
            {
                Id = notifMangaka.Id,
                Title = "Đã gửi xét duyệt Series",
                Message = notifMangaka.Content,
                Link = $"/mangaka/series/{series.Id}",
                Type = "Series_Submitted",
                CreateAt = notifMangaka.CreateAt
            };
            await _notificationPublisher.PublishNotificationPayloadAsync(mangakaId, notifMangakaPayload);
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

            var boardMemberCount = await GetActiveBoardMemberCountAsync();
            BoardVoteResolution.EnsureOddBoardMemberCount(boardMemberCount, _boardVoteSettings);

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

            await ApplyBoardVoteResolutionAsync(series);
        }

        private async Task<int> GetActiveBoardMemberCountAsync()
        {
            var activeBoardMembers = await _userRepository.FindAsync(u => u.RoleId == 3 && u.Status == UserStatus.Active);
            return activeBoardMembers.Count();
        }

        private async System.Threading.Tasks.Task ApplyBoardVoteResolutionAsync(Series series)
        {
            var boardMemberCount = await GetActiveBoardMemberCountAsync();
            BoardVoteResolution.EnsureOddBoardMemberCount(boardMemberCount, _boardVoteSettings);

            var allVotes = (await _boardVoteRepository.FindAsync(v => v.SeriesId == series.Id)).ToList();
            var approveCount = allVotes.Count(v => v.VoteType == "Approve");
            var rejectCount = allVotes.Count(v => v.VoteType == "Reject");
            var voteStartedAt = GetBoardVoteStartedAtUtc(series, allVotes);

            var outcome = BoardVoteResolution.Evaluate(new BoardVoteResolutionInput(
                boardMemberCount,
                approveCount,
                rejectCount,
                allVotes.Count,
                voteStartedAt,
                _boardVoteSettings));

            if (outcome == BoardVoteOutcome.Pending)
            {
                if (series.Status is "Pending_Approval" or "Pending_Board_Vote")
                {
                    series.Status = "Pending_Board_Vote";
                    _seriesRepository.Update(series);
                    await _unitOfWork.SaveChangesAsync();
                }

                return;
            }

            if (outcome == BoardVoteOutcome.EscalateToEditor)
            {
                series.Status = "Pending_Approval";
                _seriesRepository.Update(series);

                if (series.EditorId.HasValue)
                {
                    var notifEditor = new Notification
                    {
                        UserId = series.EditorId.Value,
                        Content = $"Bộ truyện '{series.Title}' cần Editor xử lý lại sau phiên bỏ phiếu Hội đồng (hòa hoặc hết hạn).",
                        Type = "Series_Board_Escalated",
                        IsRead = false
                    };
                    await _notificationRepository.AddAsync(notifEditor);
                    await _notificationPublisher.PublishNotificationAsync(
                        series.EditorId.Value,
                        notifEditor.Content,
                        notifEditor.Type);
                }

                await _unitOfWork.SaveChangesAsync();
                return;
            }

            if (outcome == BoardVoteOutcome.Approved)
            {
                var wasAlreadyApproved = series.Status == "Fund_Pending";
                var approveVotes = allVotes.Where(v => v.VoteType == "Approve").ToList();
                var averageBudget = approveVotes.Any()
                    ? approveVotes.Average(v => v.RecommendedBudget)
                    : series.EstimatedProductionBudget;

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

                await _unitOfWork.SaveChangesAsync();
                return;
            }

            var wasAlreadyRejected = series.Status == "Rejected";
            series.Status = "Rejected";
            _seriesRepository.Update(series);

            if (!wasAlreadyRejected)
            {
                var lastComment = allVotes.OrderByDescending(v => v.VoteAt).FirstOrDefault()?.Comment ?? string.Empty;
                var notifMangaka = new Notification
                {
                    UserId = series.MangakaId,
                    Content = string.IsNullOrWhiteSpace(lastComment)
                        ? $"Bộ truyện '{series.Title}' của bạn bị từ chối phê duyệt cấp vốn."
                        : $"Bộ truyện '{series.Title}' của bạn bị từ chối phê duyệt cấp vốn. Lý do: {lastComment}",
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

        private static DateTime? GetBoardVoteStartedAtUtc(Series series, IReadOnlyList<BoardVote> votes)
        {
            if (votes.Count > 0)
            {
                return votes.Min(v => v.VoteAt).ToUniversalTime();
            }

            return series.UpdateAt?.ToUniversalTime() ?? series.CreateAt.ToUniversalTime();
        }

        private async System.Threading.Tasks.Task ProcessStaleBoardVotesAsync(IEnumerable<Series> seriesList)
        {
            foreach (var series in seriesList.Where(s =>
                         s.Status is "Pending_Board_Vote" or "Pending_Approval"))
            {
                await ApplyBoardVoteResolutionAsync(series);
            }
        }

        private static bool CanCreateChapter(string? status) =>
            status is "Active" or "Fund_Pending" or "In Production" or "In_Production";

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

            if (!CanCreateChapter(series.Status))
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

            // Lưu từng trang ảnh qua IStorageService (MinIO / Firebase / Local theo cấu hình)
            if (dto.Pages != null && dto.Pages.Count > 0)
            {
                int pageNumber = 1;
                foreach (var file in dto.Pages)
                {
                    await using var stream = file.OpenReadStream();
                    var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType;
                    var imageUrl = await _storageService.UploadFileAsync(stream, file.FileName, contentType);

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
                ResourceFolderUrl = series.ResourceFolderUrl,
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

            var boardMemberCount = await GetActiveBoardMemberCountAsync();
            BoardVoteResolution.EnsureOddBoardMemberCount(boardMemberCount, _boardVoteSettings);

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

        public async Task<IEnumerable<SeriesReviewDto>> GetPendingReviewSeriesForEditorAsync(int editorId)
        {
            var pending = await _seriesRepository.FindAsync(s =>
                s.EditorId == editorId && s.Status == "Pending_Approval");

            var results = new List<SeriesReviewDto>();
            foreach (var series in pending.OrderByDescending(s => s.CreateAt))
            {
                results.Add(await GetSeriesReviewAsync(series.Id));
            }

            return results;
        }

        public async Task<IEnumerable<Series>> GetPendingBoardVoteSeriesAsync()
        {
            var pending = await _seriesRepository.FindAsync(s =>
                s.Status == "Pending_Board_Vote" || s.Status == "Pending_Approval");

            var list = pending.ToList();
            await ProcessStaleBoardVotesAsync(list);

            return await _seriesRepository.FindAsync(s =>
                s.Status == "Pending_Board_Vote" || s.Status == "Pending_Approval");
        }

        public async System.Threading.Tasks.Task VoteRankingAsync(int seriesId, int boardUserId, string voteType, string? comment)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "Active")
            {
                throw new ConflictException("Chỉ có thể biểu quyết trên bộ truyện đang hoạt động (Active).");
            }

            var existingVotes = await _boardVoteRepository.FindAsync(v => 
                v.SeriesId == seriesId && 
                v.BoardMemberId == boardUserId && 
                (v.VoteType == "Maintain" || v.VoteType == "Cancel"));

            if (existingVotes.Any())
            {
                throw new ConflictException("Bạn đã bỏ phiếu biểu quyết định kỳ cho bộ truyện này rồi.");
            }

            var vote = new BoardVote
            {
                SeriesId = seriesId,
                BoardMemberId = boardUserId,
                VoteType = voteType,
                RecommendedBudget = 0.00m,
                Comment = comment,
                VoteAt = DateTime.Now
            };

            await _boardVoteRepository.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync();

            var activeBoardMembers = await _userRepository.FindAsync(u => u.RoleId == 3 && u.Status == UserStatus.Active);
            int N = activeBoardMembers.Count();
            if (N == 0) N = 1;

            int cancelThreshold = (N / 2) + 1;

            var allRankingVotes = await _boardVoteRepository.FindAsync(v => 
                v.SeriesId == seriesId && 
                (v.VoteType == "Maintain" || v.VoteType == "Cancel"));

            int cancelCount = allRankingVotes.Count(v => v.VoteType == "Cancel");

            if (cancelCount >= cancelThreshold)
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    series.Status = "Cancelled";
                    _seriesRepository.Update(series);

                    var chapters = await _chapterRepository.FindAsync(c => c.SeriesId == seriesId);
                    var chapterIds = chapters.Select(c => c.Id).ToList();

                    var pages = await _pageRepository.FindAsync(p => chapterIds.Contains(p.ChapterId));
                    var pageIds = pages.Select(p => p.Id).ToList();

                    var regions = await _regionRepository.FindAsync(r => pageIds.Contains(r.PageId));
                    var regionIds = regions.Select(r => r.Id).ToList();

                    var tasks = await _tasksRepository.FindAsync(t => regionIds.Contains(t.RegionId));

                    foreach (var task in tasks)
                    {
                        if (task.Status == "Pending" || task.Status == "Draft")
                        {
                            await _walletService.ReleaseFundsAsync(task.Id, isApproved: false);
                            task.Status = "Cancelled";
                            _tasksRepository.Update(task);
                        }
                        else if (task.Status == "In_Progress" || task.Status == "Submitted" || task.Status == "Revision")
                        {
                            task.Deadline = DateTime.UtcNow.AddHours(24);
                            _tasksRepository.Update(task);

                            if (task.AssistantId.HasValue)
                            {
                                var notif = new Notification
                                {
                                    UserId = task.AssistantId.Value,
                                    Content = $"Bộ truyện '{series.Title}' bị hủy. Nhiệm vụ vẽ '{task.Description}' kích hoạt 24 giờ ân hạn để nộp bài vẽ hiện tại. Hạn chót mới: {task.Deadline:yyyy-MM-dd HH:mm}.",
                                    Type = "Task_GracePeriod",
                                    IsRead = false
                                };
                                await _notificationRepository.AddAsync(notif);
                                await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notif.Content, notif.Type);
                            }
                        }
                    }

                    var notifMangaka = new Notification
                    {
                        UserId = series.MangakaId,
                        Content = $"Bộ truyện '{series.Title}' đã bị hội đồng biểu quyết hủy do hiệu suất kém.",
                        Type = "Series_Cancelled",
                        IsRead = false
                    };
                    await _notificationRepository.AddAsync(notifMangaka);
                    await _notificationPublisher.PublishNotificationAsync(series.MangakaId, notifMangaka.Content, notifMangaka.Type);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            else
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async System.Threading.Tasks.Task RequireSeriesRevisionAsync(int seriesId, int editorId, string comment)
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
                throw new ConflictException("Chỉ có thể yêu cầu sửa đổi khi bộ truyện ở trạng thái chờ duyệt (Pending_Approval).");
            }

            series.Status = "Draft";
            _seriesRepository.Update(series);

            var notif = new Notification
            {
                UserId = series.MangakaId,
                Content = $"Yêu cầu chỉnh sửa bộ truyện '{series.Title}' từ Biên tập viên: {comment}",
                Type = "Series_Revision_Required",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _notificationPublisher.PublishNotificationAsync(series.MangakaId, notif.Content, notif.Type);
            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateSeriesAsync(int seriesId, int mangakaId, CreateSeriesDto dto)
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

            if (series.Status != "Draft" && series.Status != "Rejected")
            {
                throw new ConflictException("Chỉ có thể cập nhật thông tin bộ truyện khi ở trạng thái Draft hoặc Rejected.");
            }

            series.Title = dto.Title;
            series.Genre = dto.Genre;
            series.Synopsis = dto.Synopsis;
            series.CoverArtworkUrl = dto.CoverArtworkUrl;
            series.ResourceFolderUrl = dto.ResourceFolderUrl;
            series.EstimatedProductionBudget = dto.EstimatedProductionBudget;

            _seriesRepository.Update(series);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
