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
using MangaPublishingSystem.Application.Common;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

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
        private readonly IContractRepository _contractRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IPageRepository _pageRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly IWalletService _walletService;
        private readonly IStorageService _storageService;
        private readonly IBoardVotingService _boardVotingService;
        private readonly IPlatformWalletService _platformWalletService;

        public SeriesService(
            ISeriesRepository repository, 
            IUnitOfWork unitOfWork,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IBoardVoteRepository boardVoteRepository,
            IContractRepository contractRepository,
            IWalletRepository walletRepository,
            IChapterRepository chapterRepository,
            IPageRepository pageRepository,
            ITasksRepository tasksRepository,
            IRegionRepository regionRepository,
            IWalletService walletService,
            IStorageService storageService,
            IBoardVotingService boardVotingService,
            IPlatformWalletService platformWalletService) 
            : base(repository, unitOfWork)
        {
            _seriesRepository = repository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _boardVoteRepository = boardVoteRepository;
            _contractRepository = contractRepository;
            _walletRepository = walletRepository;
            _chapterRepository = chapterRepository;
            _pageRepository = pageRepository;
            _tasksRepository = tasksRepository;
            _regionRepository = regionRepository;
            _walletService = walletService;
            _storageService = storageService;
            _boardVotingService = boardVotingService;
            _platformWalletService = platformWalletService;
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

            var submissionNote = submitDto.SubmissionNotes?.Trim();
            if (!string.IsNullOrWhiteSpace(submissionNote))
            {
                if (submissionNote.Length > 2000)
                {
                    throw new BadRequestException("Ghi chú không được vượt quá 2000 ký tự.");
                }

                series.MangakaSubmissionNote = submissionNote;
            }

            _seriesRepository.Update(series);
            await _unitOfWork.SaveChangesAsync();

            var editorNoteSuffix = string.IsNullOrWhiteSpace(submissionNote)
                ? string.Empty
                : $" Ghi chú: {submissionNote}";

            var isResubmit = !string.IsNullOrWhiteSpace(series.EditorNote);
            var notifTitle = isResubmit ? "Series gửi lại chờ duyệt" : "Series mới chờ duyệt";
            var notifContent = isResubmit
                ? $"Mangaka đã gửi lại hồ sơ '{series.Title}' sau chỉnh sửa.{editorNoteSuffix}"
                : $"Bộ truyện mới '{series.Title}' vừa được gửi yêu cầu phê duyệt bản thảo nháp.{editorNoteSuffix}";

            // Gửi thông báo realtime cho Editor phụ trách (Phương án 3 — A04)
            var notifEditor = new Notification
            {
                UserId = series.EditorId.Value,
                Content = notifContent,
                Type = "Series_Pending_Review",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notifEditor);
            await _unitOfWork.SaveChangesAsync();

            var notifEditorPayload = new NotificationPayload
            {
                Id = notifEditor.Id,
                Title = notifTitle,
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
                throw new NotFoundException("Bo truyen khong ton tai.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Ban khong phai tac gia cua bo truyen nay.");
            }

            if (series.Status != "Approved")
            {
                throw new ConflictException("Chi co the xac nhan von sau khi Hoi dong da phe duyet goi von.");
            }

            series.Status = "Fund_Pending";
            _seriesRepository.Update(series);

            var admins = await _userRepository.FindAsync(u => u.RoleId == 1 && u.Status == UserStatus.Active);
            var content = $"Mangaka da xac nhan muc von {series.ApprovedProductionBudget:N0} VND cho bo truyen '{series.Title}'. Vui long lap hop dong.";

            foreach (var admin in admins)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = admin.Id,
                    Content = content,
                    Type = "Fund_Accepted",
                    IsRead = false
                });
            }

            await _unitOfWork.SaveChangesAsync();
            await _notificationPublisher.PublishBoardDataChangedAsync();

            foreach (var admin in admins)
            {
                await _notificationPublisher.PublishNotificationPayloadAsync(admin.Id, new NotificationPayload
                {
                    Title = "Can lap hop dong",
                    Message = content,
                    Type = "Fund_Accepted",
                    Link = "/admin/contracts",
                    CreateAt = DateTime.UtcNow
                });
            }
        }

        public async System.Threading.Tasks.Task SignContractAsync(int seriesId, int mangakaId)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bo truyen khong ton tai.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Ban khong phai tac gia cua bo truyen nay.");
            }

            if (series.Status != "Fund_Pending")
            {
                throw new ConflictException("Chi co the ky hop dong sau khi da xac nhan muc von va Admin da lap hop dong.");
            }

            var contract = await _contractRepository.GetBySeriesIdAsync(seriesId);
            if (contract == null)
            {
                throw new BadRequestException("Admin chua lap hop dong cho bo truyen nay.");
            }

            if (contract.Status != "Pending")
            {
                throw new ConflictException("Hop dong khong o trang thai cho Mangaka ky.");
            }

            var treasury = await _platformWalletService.GetTreasuryAsync();
            if (treasury.Balance < series.ApprovedProductionBudget)
            {
                throw new BadRequestException("Ngan quy he thong hien dang tam het, vui long thu lai sau.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                series.Status = "In Production";
                _seriesRepository.Update(series);

                contract.Status = "Signed";
                contract.SignedDate = DateTime.UtcNow;
                _contractRepository.Update(contract);

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

                await _platformWalletService.DisburseProductionFundAsync(
                    seriesId,
                    mangakaId,
                    series.ApprovedProductionBudget,
                    wallet);

                await _notificationPublisher.PublishWalletUpdatedAsync(mangakaId, new WalletUpdatedPayload
                {
                    WalletId = wallet.Id,
                    SetupFundBalance = wallet.SetupFundBalance,
                    WithdrawableBalance = wallet.WithdrawableBalance
                });

                var notif = new Notification
                {
                    UserId = mangakaId,
                    Content = $"Da ky hop dong cho bo truyen '{series.Title}'. So tien {series.ApprovedProductionBudget:N0} VND da duoc nap vao vi ky quy cua ban.",
                    Type = "Contract_Signed",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync();

                await _notificationPublisher.PublishNotificationPayloadAsync(mangakaId, new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Ky hop dong thanh cong",
                    Message = notif.Content,
                    Link = "/wallet",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                });

                await _notificationPublisher.PublishBoardDataChangedAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        public async System.Threading.Tasks.Task DeclineFundAsync(int seriesId, int mangakaId)
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

            if (series.Status != "Approved" && series.Status != "Fund_Pending")
            {
                throw new ConflictException("Bộ truyện không ở trạng thái chờ nhận vốn cấp phát.");
            }

            series.Status = "Draft";
            _seriesRepository.Update(series);

            if (series.EditorId.HasValue)
            {
                var content =
                    $"Mangaka đã từ chối mức ngân sách {series.ApprovedProductionBudget:N0} VND cho bộ truyện '{series.Title}'. Hồ sơ quay về bản nháp để thương lượng lại.";
                var notif = new Notification
                {
                    UserId = series.EditorId.Value,
                    Content = content,
                    Type = "Series_Fund_Declined",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync();

                await _notificationPublisher.PublishNotificationPayloadAsync(series.EditorId.Value, new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Mangaka từ chối vốn cấp phát",
                    Message = content,
                    Link = $"/editor/review/{series.Id}",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                });
            }

            await _unitOfWork.SaveChangesAsync();
            await _notificationPublisher.PublishBoardDataChangedAsync();
        }

        public async System.Threading.Tasks.Task VoteSeriesAsync(
            int seriesId,
            int boardUserId,
            string voteChoice,
            string comment,
            decimal recommendedBudget)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "Pending_Board_Vote")
            {
                throw new ConflictException(
                    series.Status == "Pending_Approval"
                        ? "Bộ truyện chưa được Editor trình lên Hội đồng, không thể bỏ phiếu."
                        : "Bộ truyện đã được chốt kết quả phê duyệt, không thể bỏ phiếu thêm.");
            }

            var existingVotes = await _boardVoteRepository.FindAsync(v =>
                v.SeriesId == seriesId && v.BoardMemberId == boardUserId);
            if (existingVotes.Any())
            {
                throw new ConflictException("Bạn đã bỏ phiếu thẩm định cho bộ truyện này rồi.");
            }

            var voteType = BoardVotingRulesCalculator.NormalizeVoteType(
                voteChoice,
                string.Equals(voteChoice, "Approve", StringComparison.OrdinalIgnoreCase),
                comment);

            if (!voteType.Equals("Approve", StringComparison.OrdinalIgnoreCase) &&
                !voteType.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Chỉ chấp nhận phiếu Approve hoặc Reject.");
            }

            if (voteType == "Approve" && recommendedBudget <= 0)
            {
                throw new BadRequestException("Vui lòng nhập ngân sách đề xuất khi phê duyệt.");
            }

            if (voteType == "Approve" && series.EstimatedProductionBudget > 0)
            {
                var minBudget = series.EstimatedProductionBudget * 0.5m;
                var maxBudget = series.EstimatedProductionBudget * 1.5m;
                if (recommendedBudget < minBudget || recommendedBudget > maxBudget)
                {
                    throw new BadRequestException(
                        $"Ngân sách đề xuất phải nằm trong khoảng 50%–150% so với ngân sách Mangaka ({minBudget:N0} – {maxBudget:N0} VND).");
                }
            }

            var vote = new BoardVote
            {
                SeriesId = seriesId,
                BoardMemberId = boardUserId,
                VoteType = voteType,
                RecommendedBudget = voteType == "Approve" ? recommendedBudget : 0.00m,
                Comment = comment,
                VoteAt = DateTime.Now
            };

            await _boardVoteRepository.AddAsync(vote);
            await _unitOfWork.SaveChangesAsync();

            var resolution = await _boardVotingService.EvaluateSeriesVotesAsync(seriesId);
            await _boardVotingService.ApplyVoteResolutionAsync(series, resolution, comment);
        }

        private static bool CanCreateChapter(string? status) =>
            status is "In Production" or "In_Production" or "Active" or "Fund_Pending";

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
                Status = "Draft"
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
                    var imageUrl = await _storageService.UploadFileAsync(stream, file.FileName, contentType, $"chapters/{chapter.Id}");

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
                EditorRecommendedBudget = series.EditorRecommendedBudget,
                ApprovedProductionBudget = series.ApprovedProductionBudget,
                Status = series.Status,
                MangakaId = series.MangakaId,
                MangakaName = mangaka?.FullName,
                EditorId = series.EditorId,
                EditorName = editor?.FullName,
                EditorNote = series.EditorNote,
                MangakaSubmissionNote = series.MangakaSubmissionNote,
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

            var config = await _boardVotingService.GetConfigAsync();
            var boardMembers = await _userRepository.FindAsync(u =>
                u.RoleId == config.BoardRoleId && u.Status == UserStatus.Active);
            if (boardMembers.Count() < 3)
            {
                throw new BadRequestException("Hội đồng phải có ít nhất 3 thành viên (N >= 3) trước khi trình hồ sơ.");
            }

            if (dto.EditorRecommendedBudget.HasValue)
            {
                if (dto.EditorRecommendedBudget.Value <= 0)
                {
                    throw new BadRequestException("Ngân sách Editor đề xuất phải lớn hơn 0.");
                }

                series.EditorRecommendedBudget = dto.EditorRecommendedBudget.Value;
            }
            else
            {
                series.EditorRecommendedBudget = 0;
            }

            if (config.ClearVotesOnResubmit)
            {
                await _boardVotingService.ClearVotesForSeriesAsync(seriesId);
            }

            series.Status = "Pending_Board_Vote";
            series.EditorNote = dto.Notes;
            _seriesRepository.Update(series);

            // Gửi thông báo cho Mangaka
            var notifSummary =
                $"#{series.Id}# Bộ truyện '{series.Title}' đã được biên tập viên chuyển lên hội đồng thẩm định.{(string.IsNullOrEmpty(dto.Notes) ? "" : $" Ghi chú: {dto.Notes}")}";

            var notifMangaka = new Notification
            {
                UserId = series.MangakaId,
                Content = TruncateNotificationContent(notifSummary),
                Type = "Series_Submitted_To_Board",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notifMangaka);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                var notifPayload = new NotificationPayload
                {
                    Id = notifMangaka.Id,
                    Title = "Series chờ Hội đồng",
                    Message = notifSummary.Replace($"#{series.Id}# ", string.Empty),
                    Link = $"/mangaka/series/{series.Id}",
                    Type = "Series_Submitted_To_Board",
                    CreateAt = notifMangaka.CreateAt == default ? DateTime.UtcNow : notifMangaka.CreateAt
                };
                await _notificationPublisher.PublishNotificationPayloadAsync(series.MangakaId, notifPayload);
            }
            catch
            {
                // Không chặn luồng nếu SignalR lỗi — dữ liệu đã lưu DB.
            }

            // Báo cho các tab Hội đồng tự động refresh
            await _notificationPublisher.PublishBoardDataChangedAsync();

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
            return await _seriesRepository.FindAsync(s => s.Status == "Pending_Board_Vote");
        }

        public async System.Threading.Tasks.Task VoteRankingAsync(int seriesId, int boardUserId, string voteType, string? comment)
        {
            var series = await _seriesRepository.GetByIdAsync(seriesId);
            if (series == null)
            {
                throw new NotFoundException("Bộ truyện không tồn tại.");
            }

            if (series.Status != "In Production" && series.Status != "Active")
            {
                throw new ConflictException("Chỉ có thể biểu quyết trên bộ truyện đang sản xuất (In Production).");
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

        public async System.Threading.Tasks.Task RequireSeriesRevisionAsync(int seriesId, int editorId, RequireSeriesRevisionDto dto)
        {
            if (dto == null)
            {
                throw new BadRequestException("Dữ liệu yêu cầu không hợp lệ.");
            }

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

            var previousBudget = series.EstimatedProductionBudget;
            if (dto.SuggestedBudget.HasValue)
            {
                series.EstimatedProductionBudget = dto.SuggestedBudget.Value;
            }

            series.EditorNote = BuildRevisionEditorNote(dto, previousBudget);
            series.MangakaSubmissionNote = null;
            series.Status = "Draft";
            _seriesRepository.Update(series);

            var notifSummary =
                $"#{series.Id}# Editor yêu cầu chỉnh sửa hồ sơ '{series.Title}'. Mở trang series để xem chi tiết.";

            var notif = new Notification
            {
                UserId = series.MangakaId,
                Content = TruncateNotificationContent(notifSummary),
                Type = "Series_Revision_Required",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync();

            try
            {
                var notifPayload = new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Editor yêu cầu chỉnh sửa hồ sơ",
                    Message = notifSummary.Replace($"#{series.Id}# ", string.Empty),
                    Link = $"/mangaka/series/{series.Id}",
                    Type = "Series_Revision_Required",
                    CreateAt = notif.CreateAt == default ? DateTime.UtcNow : notif.CreateAt
                };
                await _notificationPublisher.PublishNotificationPayloadAsync(series.MangakaId, notifPayload);
            }
            catch
            {
                // Không chặn luồng revision nếu SignalR lỗi — dữ liệu đã lưu DB.
            }
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

        public async System.Threading.Tasks.Task DeleteSeriesAsync(int id, int currentUserId, string currentUserRole)
        {
            var series = await _seriesRepository.GetByIdAsync(id);
            if (series == null)
                throw new NotFoundException("Không tìm thấy bộ truyện này.");

            if (currentUserRole == RoleNameTantouEditor)
            {
                if (series.EditorId != currentUserId)
                    throw new ForbiddenException("Bạn không phải biên tập viên phụ trách bộ truyện này.");
            }
            else if (currentUserRole != "System Admin")
            {
                if (series.MangakaId != currentUserId)
                    throw new ForbiddenException("Bạn không có quyền xóa bộ truyện này.");
            }

            _seriesRepository.Delete(series);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> HasContractAsync(int seriesId)
        {
            return await _seriesRepository.HasContractAsync(seriesId);
        }

        public async Task<Contract?> GetContractBySeriesIdAsync(int seriesId)
        {
            return await _contractRepository.GetBySeriesIdAsync(seriesId);
        }

        private static readonly Dictionary<string, string> RevisionChecklistLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["synopsis"] = "Nội dung tóm tắt rõ ràng, hấp dẫn",
            ["genre"] = "Thể loại phù hợp thị trường mục tiêu",
            ["name"] = "Phác thảo (Name) đạt chất lượng cơ bản",
            ["budget"] = "Ngân sách yêu cầu hợp lý",
        };

        private static string BuildRevisionEditorNote(RequireSeriesRevisionDto dto, decimal previousBudget)
        {
            if (dto == null)
            {
                return string.Empty;
            }

            var sections = new List<string>();

            if (dto.FailedChecklistItems is { Count: > 0 })
            {
                var checklistIds = dto.FailedChecklistItems
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                sections.Add("[CHECKLIST:" + string.Join(",", checklistIds) + "]");
                sections.Add("Cần chỉnh sửa các mục sau:");
                foreach (var itemId in checklistIds)
                {
                    if (RevisionChecklistLabels.TryGetValue(itemId, out var label))
                    {
                        sections.Add("• " + label);
                    }
                }
                sections.Add(string.Empty);
            }

            var comment = dto.Comment?.Trim();
            if (!string.IsNullOrWhiteSpace(comment))
            {
                sections.Add("Nhận xét của Editor:");
                sections.Add(comment);
            }

            if (dto.SuggestedBudget.HasValue && dto.SuggestedBudget.Value != previousBudget)
            {
                sections.Add(string.Empty);
                sections.Add(
                    $"Ngân sách đề xuất của Editor: {dto.SuggestedBudget.Value:N0} VND (Mangaka đề xuất: {previousBudget:N0} VND)");
            }

            return string.Join("\n", sections.Where(s => s != null)).Trim();
        }

        private static string TruncateNotificationContent(string content, int maxLength = 1000)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            {
                return content;
            }

            return content[..(maxLength - 1)] + "…";
        }
    }
}
