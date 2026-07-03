using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Web.Responses;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Tasks;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.Helpers;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Application.Services
{
    public class TasksService : GenericService<Tasks>, ITasksService
    {
        private readonly ITasksRepository _tasksRepository;
        private readonly IWalletService _walletService;
        private readonly IRegionRepository _regionRepository;
        private readonly IPageRepository _pageRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITaskVersionRepository _taskVersionRepository;
        private readonly IAnnotationRepository _annotationRepository;
        private readonly IAssistantProfileRepository _assistantProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IImageCompositor _imageCompositor;
        private readonly IStorageService _storageService;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly ISeriesAssistantRepository _seriesAssistantRepository;

        public TasksService(
            ITasksRepository repository, 
            IUnitOfWork unitOfWork,
            IWalletService walletService,
            IRegionRepository regionRepository,
            IPageRepository pageRepository,
            INotificationRepository notificationRepository,
            ITaskVersionRepository taskVersionRepository,
            IAnnotationRepository annotationRepository,
            IAssistantProfileRepository assistantProfileRepository,
            IUserRepository userRepository,
            IImageCompositor imageCompositor,
            IStorageService storageService,
            INotificationPublisher notificationPublisher,
            ISeriesAssistantRepository seriesAssistantRepository) 
            : base(repository, unitOfWork)
        {
            _tasksRepository = repository;
            _walletService = walletService;
            _regionRepository = regionRepository;
            _pageRepository = pageRepository;
            _notificationRepository = notificationRepository;
            _taskVersionRepository = taskVersionRepository;
            _annotationRepository = annotationRepository;
            _assistantProfileRepository = assistantProfileRepository;
            _userRepository = userRepository;
            _imageCompositor = imageCompositor;
            _storageService = storageService;
            _notificationPublisher = notificationPublisher;
            _seriesAssistantRepository = seriesAssistantRepository;
        }

        public async Task<Tasks> CreateTaskAsync(int mangakaId, CreateTaskDto createDto)
        {
            var region = await _regionRepository.GetByIdWithPageChapterSeriesAsync(createDto.RegionId);
            if (region == null)
            {
                throw new NotFoundException("Phân vùng vẽ không tồn tại trên hệ thống.");
            }

            // Chuẩn hóa: AssistantId = 0 được coi là null (Swagger tự sinh default 0 cho int?)
            var normalizedAssistantId = createDto.AssistantId.HasValue && createDto.AssistantId.Value > 0
                ? createDto.AssistantId
                : null;

            if (!normalizedAssistantId.HasValue)
            {
                throw new BadRequestException("Phải chọn Trợ lý trong nhóm dự án (Series_Assistant) để giao việc.");
            }

            var seriesId = region.Page?.Chapter?.SeriesId ?? 0;
            if (seriesId <= 0)
            {
                throw new NotFoundException("Không xác định được bộ truyện cho vùng vẽ này.");
            }

            var isTeamMember = await _seriesAssistantRepository.IsActiveMemberAsync(seriesId, normalizedAssistantId.Value);
            if (!isTeamMember)
            {
                throw new BadRequestException("Trợ lý được chọn không thuộc nhóm Active của bộ truyện này.");
            }

            var task = new Tasks
            {
                MangakaId = mangakaId,
                RegionId = createDto.RegionId,
                AssistantId = normalizedAssistantId,
                Description = createDto.Description,
                PaymentAmount = createDto.PaymentAmount,
                Deadline = createDto.Deadline,
                ZIndex_Order = createDto.ZIndex_Order,
                Status = "Draft",
                ExtensionStatus = "None"
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _tasksRepository.AddAsync(task);
                await _unitOfWork.SaveChangesAsync(); // Lưu để sinh TaskId tự động

                // Gọi khóa quỹ ví Escrow thù lao của Mangaka
                await _walletService.LockFundsAsync(mangakaId, task.PaymentAmount, task.Id);

                // Chuyển sang trạng thái Pending sau khi đã khóa quỹ an toàn
                task.Status = "Pending";
                _tasksRepository.Update(task);

                // Gửi thông báo đến trợ lý đã được chỉ định trong nhóm dự án
                if (task.AssistantId.HasValue)
                {
                    var notif = new Notification
                    {
                        UserId = task.AssistantId.Value,
                        Content = $"Bạn được giao nhiệm vụ vẽ từ tác giả thù lao {task.PaymentAmount:N0} VND. Hạn chót: {task.Deadline:yyyy-MM-dd HH:mm}.",
                        Type = "Task_Assigned",
                        IsRead = false
                    };
                    await _notificationRepository.AddAsync(notif);
                    await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

                    var notifPayload = new NotificationPayload
                    {
                        Id = notif.Id,
                        Title = "Nhiệm vụ vẽ mới được giao",
                        Message = notif.Content,
                        Link = $"/tasks/{task.Id}",
                        Type = notif.Type,
                        CreateAt = notif.CreateAt
                    };
                    await _notificationPublisher.PublishNotificationPayloadAsync(task.AssistantId.Value, notifPayload);

                    var taskStatusChanged = new TaskStatusChangedPayload
                    {
                        TaskId = task.Id,
                        Status = task.Status,
                        Message = "Nhiệm vụ vẽ mới được tạo."
                    };
                    await _notificationPublisher.PublishTaskStatusChangedAsync(task.AssistantId.Value, taskStatusChanged);
                    await _notificationPublisher.PublishTaskStatusChangedAsync(mangakaId, taskStatusChanged);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return task;
        }

        public async System.Threading.Tasks.Task CreateDisputeAsync(int taskId, int userId, string reason)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (task.MangakaId != userId && task.AssistantId != userId)
            {
                throw new ForbiddenException("Bạn không có quyền báo cáo tranh chấp nhiệm vụ này.");
            }

            if (task.Status != "Submitted" && task.Status != "Revision")
            {
                throw new ConflictException("Chỉ có thể tranh chấp nhiệm vụ đang chờ duyệt hoặc yêu cầu sửa.");
            }

            task.Status = "Disputed";
            task.FeedbackComment = reason;
            task.UpdateAt = DateTime.Now;

            _tasksRepository.Update(task);
            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task ApproveSubmissionAsync(int taskId, int mangakaId, ApproveTaskDto approveDto)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (task.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền duyệt bài nộp của nhiệm vụ này.");
            }

            if (task.Status != "Submitted")
            {
                throw new ConflictException("Nhiệm vụ không có bài nộp nào đang chờ duyệt.");
            }

            // Giải phóng ví ký quỹ chuyển tiền cho Assistant (isApproved = true)
            await _walletService.ReleaseFundsAsync(taskId, isApproved: true);

            task.Status = "Approved";
            task.Rating = approveDto.Rating;
            task.FeedbackComment = approveDto.FeedbackComment;
            _tasksRepository.Update(task);

            // Cập nhật trạng thái phiên bản nộp bài mới nhất thành Approved
            var versions = await _taskVersionRepository.FindAsync(v => v.TaskId == taskId);
            var latestVersion = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
            if (latestVersion != null)
            {
                latestVersion.Status = "Approved";
                _taskVersionRepository.Update(latestVersion);
            }

            // Auto-composite: gộp lớp vẽ Assistant đã duyệt lên trang gốc
            var region = await _regionRepository.GetByIdAsync(task.RegionId);
            if (region != null)
            {
                await RefreshPageCompositeAsync(region.PageId);
            }

            // Gửi thông báo cho Assistant
            if (task.AssistantId.HasValue)
            {
                var notif = new Notification
                {
                    UserId = task.AssistantId.Value,
                    Content = $"Tác giả đã nghiệm thu bài vẽ nhiệm vụ '{task.Description}'. Tiền thù lao đã được chuyển về ví của bạn.",
                    Type = "Task_Approved",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

                var notifPayload = new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Nhiệm vụ vẽ đã được duyệt",
                    Message = notif.Content,
                    Link = $"/tasks/{task.Id}",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                };
                await _notificationPublisher.PublishNotificationPayloadAsync(task.AssistantId.Value, notifPayload);

                var taskStatusChanged = new TaskStatusChangedPayload
                {
                    TaskId = task.Id,
                    Status = task.Status,
                    Message = "Nhiệm vụ vẽ đã được tác giả phê duyệt nghiệm thu."
                };
                await _notificationPublisher.PublishTaskStatusChangedAsync(task.AssistantId.Value, taskStatusChanged);
                await _notificationPublisher.PublishTaskStatusChangedAsync(mangakaId, taskStatusChanged);

                // Cập nhật các chỉ số của Assistant (T09)
                await UpdateAssistantProfileMetricsAsync(task.AssistantId.Value);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task RejectSubmissionAsync(int taskId, int mangakaId, RejectTaskDto rejectDto)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (task.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền từ chối bài nộp của nhiệm vụ này.");
            }

            if (task.Status != "Submitted")
            {
                throw new ConflictException("Nhiệm vụ không có bài nộp nào đang chờ đánh giá.");
            }

            var versions = await _taskVersionRepository.FindAsync(v => v.TaskId == taskId);
            var latestVersion = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
            if (latestVersion == null)
            {
                throw new ConflictException("Không tìm thấy tệp vẽ nộp nào của trợ lý.");
            }

            // Tiền ký quỹ vẫn bị khóa. Cập nhật trạng thái nhiệm vụ và cộng dồn deadline sửa
            task.Status = "Revision";
            task.Deadline = task.Deadline.AddHours(rejectDto.RevisionExtensionHours);
            task.FeedbackComment = rejectDto.FeedbackComment;
            _tasksRepository.Update(task);

            latestVersion.Status = "Rejected";
            _taskVersionRepository.Update(latestVersion);

            // Lưu điểm ghim bắt lỗi Canvas Annotation
            var annotation = new Annotation
            {
                CreatedByUserId = mangakaId,
                TaskVersionId = latestVersion.Id,
                CoordinatesJson = rejectDto.CoordinatesJson,
                Comment = rejectDto.FeedbackComment,
                Type = "Revision"
            };
            await _annotationRepository.AddAsync(annotation);

            // Gửi thông báo cho Assistant
            if (task.AssistantId.HasValue)
            {
                var notif = new Notification
                {
                    UserId = task.AssistantId.Value,
                    Content = $"Tác giả yêu cầu sửa đổi tranh cho nhiệm vụ '{task.Description}'. Vui lòng xem điểm ghim Canvas và nộp lại trong vòng {rejectDto.RevisionExtensionHours} giờ.",
                    Type = "Task_Rejected",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

                var notifPayload = new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Yêu cầu sửa đổi nhiệm vụ vẽ",
                    Message = notif.Content,
                    Link = $"/tasks/{task.Id}",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                };
                await _notificationPublisher.PublishNotificationPayloadAsync(task.AssistantId.Value, notifPayload);

                var taskStatusChanged = new TaskStatusChangedPayload
                {
                    TaskId = task.Id,
                    Status = task.Status,
                    Message = $"Tác giả yêu cầu sửa đổi bài vẽ: {rejectDto.FeedbackComment}"
                };
                await _notificationPublisher.PublishTaskStatusChangedAsync(task.AssistantId.Value, taskStatusChanged);
                await _notificationPublisher.PublishTaskStatusChangedAsync(mangakaId, taskStatusChanged);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task HandleExtensionRequestAsync(int taskId, int mangakaId, bool approve)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (task.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền quản lý yêu cầu gia hạn của nhiệm vụ này.");
            }

            if (task.ExtensionStatus != "Pending")
            {
                throw new ConflictException("Nhiệm vụ này không có yêu cầu xin gia hạn nào đang chờ xử lý.");
            }

            if (approve)
            {
                task.Deadline = task.Deadline.AddDays(task.ExtensionRequestDays ?? 0);
                task.ExtensionStatus = "Approved";

                if (task.AssistantId.HasValue)
                {
                    var notif = new Notification
                    {
                        UserId = task.AssistantId.Value,
                        Content = $"Yêu cầu xin gia hạn thêm {task.ExtensionRequestDays} ngày cho nhiệm vụ '{task.Description}' đã được tác giả chấp nhận.",
                        Type = "Extension_Approved",
                        IsRead = false
                    };
                    await _notificationRepository.AddAsync(notif);
                    await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

                    var notifPayload = new NotificationPayload
                    {
                        Id = notif.Id,
                        Title = "Yêu cầu gia hạn được chấp nhận",
                        Message = notif.Content,
                        Link = $"/tasks/{task.Id}",
                        Type = notif.Type,
                        CreateAt = notif.CreateAt
                    };
                    await _notificationPublisher.PublishNotificationPayloadAsync(task.AssistantId.Value, notifPayload);

                    var taskStatusChanged = new TaskStatusChangedPayload
                    {
                        TaskId = task.Id,
                        Status = task.Status,
                        Message = $"Yêu cầu gia hạn thêm {task.ExtensionRequestDays} ngày đã được chấp nhận."
                    };
                    await _notificationPublisher.PublishTaskStatusChangedAsync(task.AssistantId.Value, taskStatusChanged);
                    await _notificationPublisher.PublishTaskStatusChangedAsync(mangakaId, taskStatusChanged);
                }
            }
            else
            {
                task.ExtensionStatus = "Rejected";

                if (task.AssistantId.HasValue)
                {
                    var notif = new Notification
                    {
                        UserId = task.AssistantId.Value,
                        Content = $"Yêu cầu xin gia hạn cho nhiệm vụ '{task.Description}' đã bị tác giả từ chối.",
                        Type = "Extension_Rejected",
                        IsRead = false
                    };
                    await _notificationRepository.AddAsync(notif);
                    await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

                    var notifPayload = new NotificationPayload
                    {
                        Id = notif.Id,
                        Title = "Yêu cầu gia hạn bị từ chối",
                        Message = notif.Content,
                        Link = $"/tasks/{task.Id}",
                        Type = notif.Type,
                        CreateAt = notif.CreateAt
                    };
                    await _notificationPublisher.PublishNotificationPayloadAsync(task.AssistantId.Value, notifPayload);

                    var taskStatusChanged = new TaskStatusChangedPayload
                    {
                        TaskId = task.Id,
                        Status = task.Status,
                        Message = "Yêu cầu gia hạn đã bị từ chối."
                    };
                    await _notificationPublisher.PublishTaskStatusChangedAsync(task.AssistantId.Value, taskStatusChanged);
                    await _notificationPublisher.PublishTaskStatusChangedAsync(mangakaId, taskStatusChanged);
                }
            }

            _tasksRepository.Update(task);
            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task EmergencyCancelTaskAsync(int taskId, int mangakaId)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (task.MangakaId != mangakaId)
            {
                throw new ForbiddenException("Bạn không có quyền hủy khẩn cấp nhiệm vụ này.");
            }

            if (task.Status != "In_Progress")
            {
                throw new ConflictException("Chỉ có thể hủy khẩn cấp khi nhiệm vụ đang vẽ ở trạng thái In_Progress.");
            }

            // Giải phóng ví ký quỹ trả lại toàn bộ tiền về ví Mangaka (isApproved = false)
            await _walletService.ReleaseFundsAsync(taskId, isApproved: false);

            task.Status = "Cancelled";
            _tasksRepository.Update(task);

            // Thông báo trợ lý
            if (task.AssistantId.HasValue)
            {
                var notif = new Notification
                {
                    UserId = task.AssistantId.Value,
                    Content = $"Nhiệm vụ '{task.Description}' đã bị tác giả hủy bỏ khẩn cấp.",
                    Type = "Task_Cancelled",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

                var notifPayload = new NotificationPayload
                {
                    Id = notif.Id,
                    Title = "Nhiệm vụ vẽ bị hủy bỏ khẩn cấp",
                    Message = notif.Content,
                    Link = $"/tasks/{task.Id}",
                    Type = notif.Type,
                    CreateAt = notif.CreateAt
                };
                await _notificationPublisher.PublishNotificationPayloadAsync(task.AssistantId.Value, notifPayload);

                var taskStatusChanged = new TaskStatusChangedPayload
                {
                    TaskId = task.Id,
                    Status = task.Status,
                    Message = "Nhiệm vụ vẽ đã bị tác giả hủy bỏ khẩn cấp."
                };
                await _notificationPublisher.PublishTaskStatusChangedAsync(task.AssistantId.Value, taskStatusChanged);
                await _notificationPublisher.PublishTaskStatusChangedAsync(mangakaId, taskStatusChanged);

                // Cập nhật lại chỉ số Assistant
                await UpdateAssistantProfileMetricsAsync(task.AssistantId.Value);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Tasks>> GetTasksByMangakaIdAsync(int mangakaId)
        {
            return await _tasksRepository.GetMangakaTasksAsync(mangakaId);
        }

        public async Task<IEnumerable<Tasks>> GetTasksByAssistantIdAsync(int assistantId)
        {
            return await _tasksRepository.FindAsync(t => t.AssistantId == assistantId);
        }

        public async Task<IEnumerable<TaskVersion>> GetTaskVersionsAsync(int taskId)
        {
            return await _taskVersionRepository.FindAsync(v => v.TaskId == taskId);
        }

        public async Task<byte[]> GetCompositedPageAsync(int pageId)
        {
            var page = await _pageRepository.GetByIdAsync(pageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }

            var regions = await _regionRepository.FindAsync(r => r.PageId == pageId);
            var regionMap = regions.ToDictionary(r => r.Id);
            var regionIds = regions.Select(r => r.Id).ToList();

            var tasks = await _tasksRepository.FindAsync(t => regionIds.Contains(t.RegionId) && t.Status == "Approved");
            var taskList = tasks.ToList();

            var layers = new List<CompositeLayerDto>();
            foreach (var task in taskList)
            {
                var versions = await _taskVersionRepository.FindAsync(v => v.TaskId == task.Id && v.Status == "Approved");
                var approvedVer = versions.FirstOrDefault();
                if (approvedVer == null) continue;
                if (!regionMap.TryGetValue(task.RegionId, out var region)) continue;

                var (x, y, w, h) = RegionCoordinatesHelper.Parse(region.CoordinatesJson);
                // Bỏ qua Region legacy (toạ độ âm / thiếu kích thước) — tránh phủ full-page che lớp đúng
                if (w <= 0 || h <= 0 || x < 0 || y < 0) continue;

                layers.Add(new CompositeLayerDto
                {
                    OverlayUrl = approvedVer.SubmittedFileUrl,
                    ZIndex = task.ZIndex_Order,
                    X = x,
                    Y = y,
                    Width = w,
                    Height = h,
                });
            }

            return await _imageCompositor.CompositeLayersAsync(page.BaseLayerUrl ?? page.RawImageUrl, layers);
        }

        /// <summary>
        /// Tạo ảnh composite từ các Task đã Approved trên trang và lưu vào Page.CompositeImageUrl.
        /// </summary>
        public async Task<string> RefreshPageCompositeAsync(int pageId)
        {
            var compositeBytes = await GetCompositedPageAsync(pageId);
            var fileName = $"pages/{pageId}/composite-{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            using var ms = new MemoryStream(compositeBytes);
            var compositeUrl = await _storageService.UploadFileAsync(ms, fileName, "image/png");

            var page = await _pageRepository.GetByIdAsync(pageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }

            page.CompositeImageUrl = compositeUrl;
            if (page.Status == "Pending")
            {
                page.Status = "InProgress";
            }
            _pageRepository.Update(page);
            await _unitOfWork.SaveChangesAsync();
            return compositeUrl;
        }

        public async Task<PagedResult<TasksDto>> GetAvailableTasksAsync(int assistantId, GetAvailableTasksRequest request)
        {
            var pagedTasks = await _tasksRepository.GetAvailableTasksAsync(
                assistantId, request.Skill, request.PageNumber, request.PageSize);

            // Ánh xạ từ Tasks entity sang TasksDto
            var dtoItems = pagedTasks.Items.Select(t => new TasksDto
            {
                Id = t.Id,
                MangakaId = t.MangakaId,
                RegionId = t.RegionId,
                AssistantId = t.AssistantId,
                Description = t.Description,
                PaymentAmount = t.PaymentAmount,
                Deadline = t.Deadline,
                ExtensionRequestDays = t.ExtensionRequestDays,
                ExtensionReason = t.ExtensionReason,
                ExtensionStatus = t.ExtensionStatus,
                ZIndex_Order = t.ZIndex_Order,
                Status = t.Status,
                Rating = t.Rating,
                FeedbackComment = t.FeedbackComment,
                MangakaName = t.Mangaka?.FullName,
                AssistantName = t.Assistant?.FullName,
                PageNumber = t.Region?.Page?.PageNumber ?? 0,
                PageId = t.Region?.PageId ?? 0,
                PageImageUrl = t.Region?.Page?.RawImageUrl,
                RegionName = t.Region?.Name,
                RegionCoordinatesJson = t.Region?.CoordinatesJson,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            }).ToList();

            return new PagedResult<TasksDto>(
                dtoItems,
                pagedTasks.PageNumber,
                pagedTasks.PageSize,
                pagedTasks.TotalItems,
                pagedTasks.TotalPages);
        }

        public async Task<PagedResult<TasksDto>> GetAssistantTasksAsync(int assistantId, GetAssistantTasksRequest request)
        {
            var pagedTasks = await _tasksRepository.GetAssistantTasksAsync(
                assistantId, request.Status, request.PageNumber, request.PageSize);

            // Ánh xạ từ Tasks entity sang TasksDto
            var dtoItems = pagedTasks.Items.Select(t => new TasksDto
            {
                Id = t.Id,
                MangakaId = t.MangakaId,
                RegionId = t.RegionId,
                AssistantId = t.AssistantId,
                Description = t.Description,
                PaymentAmount = t.PaymentAmount,
                Deadline = t.Deadline,
                ExtensionRequestDays = t.ExtensionRequestDays,
                ExtensionReason = t.ExtensionReason,
                ExtensionStatus = t.ExtensionStatus,
                ZIndex_Order = t.ZIndex_Order,
                Status = t.Status,
                Rating = t.Rating,
                FeedbackComment = t.FeedbackComment,
                MangakaName = t.Mangaka?.FullName,
                AssistantName = t.Assistant?.FullName,
                PageNumber = t.Region?.Page?.PageNumber ?? 0,
                PageId = t.Region?.PageId ?? 0,
                PageImageUrl = t.Region?.Page?.RawImageUrl,
                RegionName = t.Region?.Name,
                RegionCoordinatesJson = t.Region?.CoordinatesJson,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            }).ToList();

            return new PagedResult<TasksDto>(
                dtoItems,
                pagedTasks.PageNumber,
                pagedTasks.PageSize,
                pagedTasks.TotalItems,
                pagedTasks.TotalPages);
        }

        public async Task<TasksDto> GetTaskDetailsByIdAsync(int taskId, int userId, bool isMangaka)
        {
            var task = await _tasksRepository.GetTaskByIdWithDetailsAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (isMangaka)
            {
                if (task.MangakaId != userId)
                {
                    throw new ForbiddenException("Bạn không có quyền xem nhiệm vụ này.");
                }
            }
            else
            {
                var isAssignedToMe = task.AssistantId == userId;
                if (!isAssignedToMe)
                {
                    throw new ForbiddenException("Bạn không có quyền xem nhiệm vụ này.");
                }
            }

            return MapTaskEntityToDto(task);
        }

        private static TasksDto MapTaskEntityToDto(Tasks t)
        {
            var page = t.Region?.Page;
            var chapter = page?.Chapter;
            return new TasksDto
            {
                Id = t.Id,
                MangakaId = t.MangakaId,
                RegionId = t.RegionId,
                AssistantId = t.AssistantId,
                Description = t.Description,
                PaymentAmount = t.PaymentAmount,
                Deadline = t.Deadline,
                ExtensionRequestDays = t.ExtensionRequestDays,
                ExtensionReason = t.ExtensionReason,
                ExtensionStatus = t.ExtensionStatus,
                ZIndex_Order = t.ZIndex_Order,
                Status = t.Status,
                Rating = t.Rating,
                FeedbackComment = t.FeedbackComment,
                MangakaName = t.Mangaka?.FullName,
                AssistantName = t.Assistant?.FullName,
                SeriesId = chapter?.SeriesId ?? 0,
                SeriesTitle = chapter?.Series?.Title,
                ChapterId = chapter?.Id ?? 0,
                ChapterNumber = chapter?.ChapterNumber ?? 0,
                ChapterTitle = chapter?.Title,
                PageId = page?.Id ?? 0,
                PageNumber = page?.PageNumber ?? 0,
                PageImageUrl = page?.RawImageUrl,
                BaseLayerUrl = page?.BaseLayerUrl ?? page?.RawImageUrl,
                RegionName = t.Region?.Name,
                RegionCoordinatesJson = t.Region?.CoordinatesJson,
                CreateAt = t.CreateAt,
                UpdateAt = t.UpdateAt
            };
        }

        private async System.Threading.Tasks.Task UpdateAssistantProfileMetricsAsync(int assistantId)
        {
            var profileList = await _assistantProfileRepository.FindAsync(p => p.AssistantId == assistantId);
            var profile = profileList.FirstOrDefault();
            if (profile == null) return;

            var allTasks = await _tasksRepository.FindAsync(t => t.AssistantId == assistantId);
            var tasksList = allTasks.ToList();

            var approvedTasks = tasksList.Where(t => t.Status == "Approved").ToList();
            profile.TotalCompletedTasks = approvedTasks.Count;

            if (approvedTasks.Count > 0)
            {
                int onTimeCount = 0;
                foreach (var task in approvedTasks)
                {
                    var versions = await _taskVersionRepository.FindAsync(v => v.TaskId == task.Id && v.Status == "Approved");
                    var approvedVer = versions.FirstOrDefault();
                    if (approvedVer != null && approvedVer.SubmittedAt <= task.Deadline)
                    {
                        onTimeCount++;
                    }
                    else if (approvedVer == null)
                    {
                        var allVer = await _taskVersionRepository.FindAsync(v => v.TaskId == task.Id);
                        if (allVer.Any(v => v.SubmittedAt <= task.Deadline))
                        {
                            onTimeCount++;
                        }
                    }
                }
                profile.OnTimeRate = ((decimal)onTimeCount / approvedTasks.Count) * 100m;

                var ratedTasks = approvedTasks.Where(t => t.Rating.HasValue).ToList();
                profile.AverageRating = ratedTasks.Count > 0 ? (decimal)ratedTasks.Average(t => t.Rating!.Value) : 0.00m;
            }
            else
            {
                profile.OnTimeRate = 0.00m;
                profile.AverageRating = 0.00m;
            }

            profile.CurrentActiveTasks = tasksList.Count(t => t.Status == "In_Progress" || t.Status == "Submitted" || t.Status == "Revision" || t.Status == "Pending");

            _assistantProfileRepository.Update(profile);
        }

        public async System.Threading.Tasks.Task AcceptTaskAsync(int taskId, int assistantId)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (task.Status != "Pending")
            {
                throw new ConflictException("Nhiệm vụ không ở trạng thái chờ nhận.");
            }

            if (task.AssistantId.HasValue && task.AssistantId.Value != assistantId)
            {
                throw new ForbiddenException("Nhiệm vụ đã được giao cho trợ lý khác.");
            }

            task.AssistantId = assistantId;
            task.Status = "In_Progress";
            _tasksRepository.Update(task);

            // Gửi thông báo đến Mangaka
            var notif = new Notification
            {
                UserId = task.MangakaId,
                Content = $"Trợ lý đã đồng ý nhận nhiệm vụ vẽ '{task.Description}'. Trạng thái hiện tại: Đang thực hiện.",
                Type = "Task_Accepted",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

            var notifPayload = new NotificationPayload
            {
                Id = notif.Id,
                Title = "Trợ lý nhận nhiệm vụ vẽ",
                Message = notif.Content,
                Link = $"/tasks/{task.Id}",
                Type = notif.Type,
                CreateAt = notif.CreateAt
            };
            await _notificationPublisher.PublishNotificationPayloadAsync(task.MangakaId, notifPayload);

            var taskStatusChanged = new TaskStatusChangedPayload
            {
                TaskId = task.Id,
                Status = task.Status,
                Message = "Trợ lý đã nhận nhiệm vụ vẽ."
            };
            await _notificationPublisher.PublishTaskStatusChangedAsync(task.MangakaId, taskStatusChanged);
            await _notificationPublisher.PublishTaskStatusChangedAsync(assistantId, taskStatusChanged);

            // Cập nhật các chỉ số của Assistant
            await UpdateAssistantProfileMetricsAsync(assistantId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task SubmitTaskAsync(int taskId, int assistantId, SubmitTaskDto dto)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (task.AssistantId != assistantId)
            {
                throw new ForbiddenException("Bạn không có quyền nộp bài cho nhiệm vụ này.");
            }

            if (task.Status != "In_Progress" && task.Status != "Revision")
            {
                throw new ConflictException("Chỉ có thể nộp bài khi nhiệm vụ đang thực hiện hoặc đang yêu cầu sửa đổi.");
            }

            var versions = await _taskVersionRepository.FindAsync(v => v.TaskId == taskId);
            var maxVersionNum = versions.Any() ? versions.Max(v => v.VersionNumber) : 0;

            var newVersion = new TaskVersion
            {
                TaskId = taskId,
                VersionNumber = maxVersionNum + 1,
                SubmittedFileUrl = dto.SubmittedFileUrl,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow
            };
            await _taskVersionRepository.AddAsync(newVersion);

            task.Status = "Submitted";
            _tasksRepository.Update(task);

            // Gửi thông báo đến Mangaka
            var notif = new Notification
            {
                UserId = task.MangakaId,
                Content = $"Trợ lý đã nộp bản vẽ mới (Phiên bản {newVersion.VersionNumber}) cho nhiệm vụ '{task.Description}'. Vui lòng phê duyệt hoặc từ chối.",
                Type = "Task_Submitted",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

            var notifPayload = new NotificationPayload
            {
                Id = notif.Id,
                Title = "Trợ lý nộp bản vẽ",
                Message = notif.Content,
                Link = $"/tasks/{task.Id}",
                Type = notif.Type,
                CreateAt = notif.CreateAt
            };
            await _notificationPublisher.PublishNotificationPayloadAsync(task.MangakaId, notifPayload);

            var taskStatusChanged = new TaskStatusChangedPayload
            {
                TaskId = task.Id,
                Status = task.Status,
                Message = $"Trợ lý đã nộp bản vẽ phiên bản {newVersion.VersionNumber}."
            };
            await _notificationPublisher.PublishTaskStatusChangedAsync(task.MangakaId, taskStatusChanged);
            await _notificationPublisher.PublishTaskStatusChangedAsync(assistantId, taskStatusChanged);

            // Cập nhật các chỉ số của Assistant
            await UpdateAssistantProfileMetricsAsync(assistantId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task RequestExtensionAsync(int taskId, int assistantId, RequestExtensionDto dto)
        {
            var task = await _tasksRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new NotFoundException("Nhiệm vụ không tồn tại.");
            }

            if (task.AssistantId != assistantId)
            {
                throw new ForbiddenException("Bạn không có quyền xin gia hạn cho nhiệm vụ này.");
            }

            if (task.Status != "In_Progress" && task.Status != "Revision")
            {
                throw new ConflictException("Chỉ có thể xin gia hạn khi nhiệm vụ đang thực hiện hoặc đang yêu cầu sửa đổi.");
            }

            if (task.ExtensionStatus != "None")
            {
                throw new ConflictException("Nhiệm vụ này đã từng yêu cầu gia hạn trước đó. Chỉ được xin gia hạn tối đa 1 lần.");
            }

            task.ExtensionRequestDays = dto.Days;
            task.ExtensionReason = dto.Reason;
            task.ExtensionStatus = "Pending";
            _tasksRepository.Update(task);

            // Gửi thông báo đến Mangaka
            var notif = new Notification
            {
                UserId = task.MangakaId,
                Content = $"Trợ lý đã gửi yêu cầu gia hạn thêm {dto.Days} ngày cho nhiệm vụ '{task.Description}'. Lý do: '{dto.Reason}'.",
                Type = "Extension_Requested",
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            await _unitOfWork.SaveChangesAsync(); // Để sinh Id cho notif

            var notifPayload = new NotificationPayload
            {
                Id = notif.Id,
                Title = "Trợ lý xin gia hạn nhiệm vụ vẽ",
                Message = notif.Content,
                Link = $"/tasks/{task.Id}",
                Type = notif.Type,
                CreateAt = notif.CreateAt
            };
            await _notificationPublisher.PublishNotificationPayloadAsync(task.MangakaId, notifPayload);

            var taskStatusChanged = new TaskStatusChangedPayload
            {
                TaskId = task.Id,
                Status = task.Status,
                Message = $"Trợ lý yêu cầu gia hạn thêm {dto.Days} ngày."
            };
            await _notificationPublisher.PublishTaskStatusChangedAsync(task.MangakaId, taskStatusChanged);
            await _notificationPublisher.PublishTaskStatusChangedAsync(assistantId, taskStatusChanged);
        }
    }
}