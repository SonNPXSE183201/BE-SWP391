using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Http;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Application.DTOs.Reviews;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.DTOs.Chapters;

namespace MangaPublishingSystem.Application.Services
{
    public class ChapterService : GenericService<Chapter>, IChapterService
    {
        private static readonly HashSet<string> OpenTaskStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pending", "In_Progress", "Submitted", "Revision", "Disputed"
        };

        private static readonly HashSet<string> SubmitAllowedChapterStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Draft", "Revision", "Rejected"
        };

        private readonly IChapterRepository _chapterRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IPageRepository _pageRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly ITasksService _tasksService;
        private readonly IStorageService _storageService;

        public ChapterService(
            IChapterRepository repository, 
            IUnitOfWork unitOfWork,
            ISeriesRepository seriesRepository,
            IContractRepository contractRepository,
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            IPageRepository pageRepository,
            IRegionRepository regionRepository,
            ITasksRepository tasksRepository,
            ITasksService tasksService,
            IStorageService storageService) 
            : base(repository, unitOfWork)
        {
            _chapterRepository = repository;
            _seriesRepository = seriesRepository;
            _contractRepository = contractRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _pageRepository = pageRepository;
            _regionRepository = regionRepository;
            _tasksRepository = tasksRepository;
            _tasksService = tasksService;
            _storageService = storageService;
        }

        public async Task<IEnumerable<Chapter>> GetChaptersBySeriesIdAsync(int seriesId)
        {
            return await _repository.FindAsync(c => c.SeriesId == seriesId);
        }

        public async System.Threading.Tasks.Task ApproveChapterAsync(int chapterId, int editorId, ApproveChapterDto dto)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }
            if (chapter.Status == "Approved")
            {
                throw new BadRequestException("Chapter đã được duyệt trước đó.");
            }

            var contract = await _contractRepository.GetEffectiveContractWithAddendumsBySeriesIdAsync(chapter.SeriesId);
            if (contract == null)
            {
                throw new BadRequestException("Không tìm thấy hợp đồng hợp lệ cho bộ truyện này.");
            }

            var applicablePrice = await GetApplicableGenkouryoPriceAsync(chapter.SeriesId);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                chapter.Status = "Approved";
                chapter.ValidPageCount = dto.ValidPageCount;
                chapter.AppliedGenkouryoPrice = applicablePrice;
                chapter.QcChecklistData = dto.QcChecklistData;
                _repository.Update(chapter);

                decimal amount = dto.ValidPageCount * applicablePrice;

                var wallet = await _walletRepository.GetWalletByUserIdAsync(contract.UserId);
                if (wallet == null)
                {
                    throw new NotFoundException("Không tìm thấy ví của tác giả.");
                }
                wallet.WithdrawableBalance += amount;
                _walletRepository.Update(wallet);

                var transaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = "Genkouryo_Payment",
                    ReferenceId = chapterId,
                    Amount = amount,
                    WithdrawableAmount = amount,
                    Status = "Success",
                    ToUserId = contract.UserId
                };
                await _transactionRepository.AddAsync(transaction);

                var notif = new Notification
                {
                    UserId = contract.UserId,
                    Content = $"Chapter {chapter.ChapterNumber}: {chapter.Title} đã được duyệt. Nhuận bút giải ngân: {amount:N0} VND.",
                    Type = "Genkouryo_Paid",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var notifPayload = new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Duyệt Chapter & Giải ngân",
                    Message = notif.Content,
                    Link = $"/chapters/{chapterId}",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                };
                await _notificationPublisher.PublishNotificationPayloadAsync(contract.UserId, notifPayload);

                await _notificationPublisher.PublishWalletUpdatedAsync(contract.UserId, new WalletUpdatedPayload
                {
                    WalletId = wallet.Id,
                    SetupFundBalance = wallet.SetupFundBalance,
                    WithdrawableBalance = wallet.WithdrawableBalance
                });
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async System.Threading.Tasks.Task RejectChapterAsync(int chapterId, int editorId, RejectChapterDto dto)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }
            if (chapter.Status == "Approved")
            {
                throw new BadRequestException("Chapter đã được duyệt trước đó, không thể từ chối.");
            }

            var series = await _seriesRepository.GetByIdAsync(chapter.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện liên kết với chapter này.");
            }

            chapter.Status = "Revision";
            _repository.Update(chapter);
            await _unitOfWork.SaveChangesAsync();

            var notif = new Notification
            {
                UserId = series.MangakaId,
                Content = $"Chapter {chapter.ChapterNumber}: {chapter.Title} bị từ chối duyệt. Lý do: {dto.FeedbackComment}",
                Type = "Chapter_Rejected",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync();

            var notifPayload = new NotificationPayload
            {
                Id = notif.Id,
                Title = "Chapter bị từ chối duyệt",
                Message = notif.Content,
                Link = $"/chapters/{chapterId}",
                Type = notif.Type,
                CreateAt = notif.CreateAt
            };
            await _notificationPublisher.PublishNotificationPayloadAsync(series.MangakaId, notifPayload);
        }

        public async Task<IEnumerable<Chapter>> GetPendingReviewChaptersForEditorAsync(int editorId)
        {
            var chapters = (await _chapterRepository.GetPendingReviewChaptersWithDetailsAsync(editorId)).ToList();
            foreach (var chapter in chapters)
            {
                await EnrichGenkouryoPreviewAsync(chapter);
            }
            return chapters;
        }

        public async System.Threading.Tasks.Task UpdateDeadlineAsync(int chapterId, DateTime deadline)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }
            chapter.SubmissionDeadline = deadline;
            _repository.Update(chapter);
            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task PublishChapterAsync(int chapterId)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }
            if (chapter.Status != "Approved")
            {
                throw new BadRequestException("Chỉ những chapter đã được phê duyệt (Approved) mới có thể xuất bản.");
            }
            chapter.Status = "Published";
            _repository.Update(chapter);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Page>> AddPagesAsync(int chapterId, int mangakaId, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                throw new BadRequestException("Vui lòng chọn ít nhất 1 trang để tải lên.");
            }

            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }

            var series = await _seriesRepository.GetByIdAsync(chapter.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện liên kết với chapter này.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không phải tác giả của bộ truyện này.");
            }

            if (chapter.Status is "Approved" or "Published")
            {
                throw new ConflictException("Chapter đã được duyệt/xuất bản, không thể thêm trang mới.");
            }

            // Tính số thứ tự trang kế tiếp dựa trên các trang hiện có
            var existingPages = await _pageRepository.FindAsync(p => p.ChapterId == chapterId);
            int nextPageNumber = existingPages.Any() ? existingPages.Max(p => p.PageNumber) + 1 : 1;

            var addedPages = new List<Page>();
            foreach (var file in files)
            {
                await using var stream = file.OpenReadStream();
                var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType;
                var imageUrl = await _storageService.UploadFileAsync(stream, file.FileName, contentType, $"chapters/{chapter.Id}");

                var page = new Page
                {
                    ChapterId = chapter.Id,
                    PageNumber = nextPageNumber,
                    RawImageUrl = imageUrl,
                    BaseLayerUrl = imageUrl,
                    Status = "Pending",
                    IsApproved = false
                };

                await _pageRepository.AddAsync(page);
                addedPages.Add(page);
                nextPageNumber++;
            }

            // Đồng bộ số trang của chapter (giữ bằng tổng số trang cho tới khi Editor chốt số hợp lệ)
            chapter.ValidPageCount = existingPages.Count() + addedPages.Count;
            _repository.Update(chapter);

            await _unitOfWork.SaveChangesAsync();

            return addedPages;
        }

        public async Task<Chapter?> GetChapterWithDetailsAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetChapterWithDetailsByIdAsync(chapterId);
            if (chapter != null)
            {
                await EnrichGenkouryoPreviewAsync(chapter);
            }
            return chapter;
        }

        private async Task<decimal> GetApplicableGenkouryoPriceAsync(int seriesId)
        {
            var contract = await _contractRepository.GetEffectiveContractWithAddendumsBySeriesIdAsync(seriesId);
            if (contract == null)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            var latestAddendum = contract.ContractAddendums?
                .Where(a => a.EffectiveDate <= now)
                .OrderByDescending(a => a.EffectiveDate)
                .FirstOrDefault();

            return latestAddendum?.NewGenkouryoPrice ?? contract.BaseGenkouryoPrice;
        }

        private async System.Threading.Tasks.Task EnrichGenkouryoPreviewAsync(Chapter chapter)
        {
            if (chapter.AppliedGenkouryoPrice > 0)
            {
                return;
            }

            var price = await GetApplicableGenkouryoPriceAsync(chapter.SeriesId);
            if (price > 0)
            {
                chapter.AppliedGenkouryoPrice = price;
            }
        }

        public async Task<Page> MarkPageAsReadyAsync(int pageId, int mangakaId)
        {
            var page = await _pageRepository.GetByIdAsync(pageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }

            var chapter = await _chapterRepository.GetByIdAsync(page.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chương truyện.");
            }

            var series = await _seriesRepository.GetByIdAsync(chapter.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không phải tác giả của bộ truyện này.");
            }

            if (!SubmitAllowedChapterStatuses.Contains(chapter.Status))
            {
                throw new ConflictException("Chapter không còn ở trạng thái cho phép chỉnh sửa.");
            }

            if (string.Equals(page.Status, "Composited", StringComparison.OrdinalIgnoreCase))
            {
                return page;
            }

            var regions = (await _regionRepository.FindAsync(r => r.PageId == pageId)).ToList();
            if (regions.Count > 0)
            {
                var regionIds = regions.Select(r => r.Id).ToList();
                var allTasks = (await _tasksRepository.FindAsync(t => regionIds.Contains(t.RegionId))).ToList();

                var tasks = allTasks.GroupBy(t => t.RegionId)
                                    .Select(g => g.OrderByDescending(t => t.Id).First())
                                    .ToList();

                if (tasks.Any(t => OpenTaskStatuses.Contains(t.Status)))
                {
                    throw new BadRequestException(
                        "Trang còn task Assistant chưa xong. Hãy nghiệm thu task hoặc xóa vùng trước khi đánh dấu.");
                }

                if (tasks.Count == 0)
                {
                    throw new BadRequestException(
                        "Trang còn vùng Canvas chưa giao task. Xóa vùng nếu không cần Assistant, hoặc giao task và duyệt xong.");
                }

                if (tasks.Any(t => !string.Equals(t.Status, "Approved", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new BadRequestException(
                        "Trang còn task chưa được nghiệm thu. Hoàn thành trên Canvas trước khi đánh dấu.");
                }

                await _tasksService.RefreshPageCompositeAsync(pageId);
                page = await _pageRepository.GetByIdAsync(pageId)
                    ?? throw new NotFoundException("Trang truyện không tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(page.CompositeImageUrl))
            {
                page.CompositeImageUrl = page.RawImageUrl ?? page.BaseLayerUrl;
            }

            page.Status = "Composited";
            _pageRepository.Update(page);
            await _unitOfWork.SaveChangesAsync();
            return page;
        }

        public async Task<Page> UnmarkPageAsReadyAsync(int pageId, int mangakaId)
        {
            var page = await _pageRepository.GetByIdAsync(pageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }

            var chapter = await _chapterRepository.GetByIdAsync(page.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chương truyện.");
            }

            var series = await _seriesRepository.GetByIdAsync(chapter.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không phải tác giả của bộ truyện này.");
            }

            if (!SubmitAllowedChapterStatuses.Contains(chapter.Status))
            {
                throw new ConflictException("Chapter không còn ở trạng thái cho phép chỉnh sửa.");
            }

            page.Status = "InProgress";
            _pageRepository.Update(page);
            await _unitOfWork.SaveChangesAsync();
            return page;
        }

        public async Task<Page> ReplacePageImageAsync(int pageId, int mangakaId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Vui lòng chọn ảnh trang để tải lên.");
            }

            var page = await _pageRepository.GetByIdAsync(pageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }

            var chapter = await _chapterRepository.GetByIdAsync(page.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chương truyện.");
            }

            var series = await _seriesRepository.GetByIdAsync(chapter.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không phải tác giả của bộ truyện này.");
            }

            if (chapter.Status is "Approved" or "Published")
            {
                throw new ConflictException("Chapter đã được duyệt/xuất bản, không thể thay ảnh trang.");
            }

            if (!SubmitAllowedChapterStatuses.Contains(chapter.Status))
            {
                throw new ConflictException("Chapter không còn ở trạng thái cho phép chỉnh sửa.");
            }

            var regions = (await _regionRepository.FindAsync(r => r.PageId == pageId)).ToList();
            if (regions.Count > 0)
            {
                var regionIds = regions.Select(r => r.Id).ToList();
                var tasks = (await _tasksRepository.FindAsync(t => regionIds.Contains(t.RegionId))).ToList();

                if (tasks.Any(t => OpenTaskStatuses.Contains(t.Status)))
                {
                    throw new BadRequestException(
                        "Trang còn task Assistant chưa xong. Hãy nghiệm thu hoặc hủy task trước khi thay ảnh.");
                }

                if (tasks.Count > 0)
                {
                    throw new BadRequestException(
                        "Trang đã có công việc Assistant. Hãy xử lý trên Canvas hoặc xóa vùng chưa giao task trước khi thay ảnh.");
                }

                foreach (var region in regions)
                {
                    _regionRepository.Delete(region);
                }
            }

            await using var stream = file.OpenReadStream();
            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType;
            var imageUrl = await _storageService.UploadFileAsync(stream, file.FileName, contentType, $"pages/{pageId}");

            page.RawImageUrl = imageUrl;
            page.BaseLayerUrl = imageUrl;
            page.CompositeImageUrl = null;
            page.Status = "Pending";
            page.IsApproved = false;
            page.UpdateAt = DateTime.UtcNow;
            _pageRepository.Update(page);
            await _unitOfWork.SaveChangesAsync();

            return page;
        }

        public async Task<ChapterProductionReadinessDto> GetProductionReadinessAsync(int chapterId, int mangakaId)
        {
            var (chapter, series, pages, _) = await LoadChapterProductionContextAsync(chapterId, mangakaId);
            return await BuildProductionReadinessAsync(chapter, pages, ensureComposites: false);
        }

        public async System.Threading.Tasks.Task<Chapter> SubmitChapterForReviewAsync(int chapterId, int mangakaId)
        {
            var (chapter, series, pages, _) = await LoadChapterProductionContextAsync(chapterId, mangakaId);

            if (!SubmitAllowedChapterStatuses.Contains(chapter.Status))
            {
                throw new ConflictException("Chapter không ở trạng thái cho phép nộp lên Editor.");
            }

            var readiness = await BuildProductionReadinessAsync(chapter, pages, ensureComposites: true);
            if (!readiness.CanSubmit)
            {
                var message = readiness.Blockers.Count > 0
                    ? string.Join(" ", readiness.Blockers)
                    : "Chapter chưa sẵn sàng để nộp lên Editor.";
                throw new BadRequestException(message);
            }

            chapter.Status = "Pending_Review";
            _repository.Update(chapter);
            await _unitOfWork.SaveChangesAsync();

            if (series.EditorId.HasValue)
            {
                var notif = new Notification
                {
                    UserId = series.EditorId.Value,
                    Content = $"Tác giả đã nộp chapter '{chapter.Title}' (Chapter {chapter.ChapterNumber}) của bộ '{series.Title}' lên hàng đợi biên tập.",
                    Type = "Chapter_Submitted",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync();

                var notifPayload = new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Chapter chờ biên tập",
                    Message = notif.Content,
                    Link = "/editor/review",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                };
                await _notificationPublisher.PublishNotificationPayloadAsync(series.EditorId.Value, notifPayload);
            }

            return chapter;
        }

        private async Task<(Chapter chapter, Series series, List<Page> pages, List<Tasks> tasks)> LoadChapterProductionContextAsync(
            int chapterId,
            int mangakaId)
        {
            var chapter = await _repository.GetByIdAsync(chapterId);
            if (chapter == null)
            {
                throw new NotFoundException("Không tìm thấy chapter.");
            }

            var series = await _seriesRepository.GetByIdAsync(chapter.SeriesId);
            if (series == null)
            {
                throw new NotFoundException("Không tìm thấy bộ truyện liên kết với chapter này.");
            }

            if (series.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không phải tác giả của bộ truyện này.");
            }

            var pages = (await _pageRepository.FindAsync(p => p.ChapterId == chapterId))
                .OrderBy(p => p.PageNumber)
                .ToList();

            var pageIds = pages.Select(p => p.Id).ToList();
            var regions = pageIds.Count == 0
                ? new List<Region>()
                : (await _regionRepository.FindAsync(r => pageIds.Contains(r.PageId))).ToList();

            var regionIds = regions.Select(r => r.Id).ToList();
            var tasks = regionIds.Count == 0
                ? new List<Tasks>()
                : (await _tasksRepository.FindAsync(t => regionIds.Contains(t.RegionId))).ToList();

            return (chapter, series, pages, tasks);
        }

        private async Task<ChapterProductionReadinessDto> BuildProductionReadinessAsync(
            Chapter chapter,
            List<Page> pages,
            bool ensureComposites)
        {
            var blockers = new List<string>();
            var checks = new List<ChapterProductionCheckDto>();
            var openTaskCount = 0;
            var pagesReady = 0;

            var hasPages = pages.Count > 0;
            checks.Add(new ChapterProductionCheckDto
            {
                Key = "pages",
                Label = "Có ít nhất 1 trang bản thảo",
                Passed = hasPages,
                Detail = hasPages ? $"{pages.Count} trang" : null
            });
            if (!hasPages)
            {
                blockers.Add("Chapter chưa có trang bản thảo.");
            }

            var pageIds = pages.Select(p => p.Id).ToList();
            var regions = pageIds.Count == 0
                ? new List<Region>()
                : (await _regionRepository.FindAsync(r => pageIds.Contains(r.PageId))).ToList();
            var regionsByPage = regions.GroupBy(r => r.PageId).ToDictionary(g => g.Key, g => g.ToList());

            var regionIds = regions.Select(r => r.Id).ToList();
            var allTasks = regionIds.Count == 0
                ? new List<Tasks>()
                : (await _tasksRepository.FindAsync(t => regionIds.Contains(t.RegionId))).ToList();
            
            var tasks = allTasks.GroupBy(t => t.RegionId)
                                .Select(g => g.OrderByDescending(t => t.Id).First())
                                .ToList();

            var tasksByRegion = tasks.GroupBy(t => t.RegionId).ToDictionary(g => g.Key, g => g.ToList());

            openTaskCount = tasks.Count(t => OpenTaskStatuses.Contains(t.Status));
            var tasksClear = openTaskCount == 0;
            checks.Add(new ChapterProductionCheckDto
            {
                Key = "tasks",
                Label = "Không còn task Assistant đang chờ xử lý",
                Passed = tasksClear,
                Detail = tasksClear ? null : $"{openTaskCount} task chưa hoàn thành"
            });
            if (!tasksClear)
            {
                blockers.Add($"Còn {openTaskCount} task Assistant chưa hoàn thành (cần nghiệm thu hoặc xử lý trên Canvas).");
            }

            foreach (var page in pages)
            {
                if (string.Equals(page.Status, "Composited", StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(page.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    pagesReady++;
                    continue;
                }

                var pageRegions = regionsByPage.GetValueOrDefault(page.Id) ?? new List<Region>();
                if (pageRegions.Count == 0)
                {
                    blockers.Add($"Trang {page.PageNumber}: chưa được đánh dấu sẵn sàng.");
                    continue;
                }

                var unassignedRegions = pageRegions.Where(r => !tasksByRegion.ContainsKey(r.Id)).ToList();
                if (unassignedRegions.Count > 0)
                {
                    blockers.Add($"Trang {page.PageNumber}: còn {unassignedRegions.Count} vùng chưa giao task.");
                    continue;
                }

                var pageTasks = pageRegions.SelectMany(r => tasksByRegion[r.Id]).ToList();
                if (pageTasks.Any(t => OpenTaskStatuses.Contains(t.Status)))
                {
                    // Global open task error handles this, just skip the page level error
                    continue;
                }

                if (pageTasks.Any(t => !string.Equals(t.Status, "Approved", StringComparison.OrdinalIgnoreCase)))
                {
                    blockers.Add($"Trang {page.PageNumber}: còn task chưa được nghiệm thu.");
                    continue;
                }

                blockers.Add($"Trang {page.PageNumber}: đã duyệt các task nhưng chưa được đánh dấu sẵn sàng.");
            }

            var compositesReady = pagesReady == pages.Count && pages.Count > 0;
            checks.Add(new ChapterProductionCheckDto
            {
                Key = "composite",
                Label = "Mọi trang đã sẵn sàng nộp (bản gộp hoặc không cần Assistant)",
                Passed = compositesReady,
                Detail = compositesReady ? null : $"{pagesReady}/{pages.Count} trang sẵn sàng"
            });

            return new ChapterProductionReadinessDto
            {
                ChapterId = chapter.Id,
                Status = chapter.Status,
                CanSubmit = blockers.Count == 0 && hasPages,
                TotalPages = pages.Count,
                PagesReady = pagesReady,
                OpenTaskCount = openTaskCount,
                Checks = checks,
                Blockers = blockers
            };
        }
    }
}