using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using MangaPublishingSystem.Domain.Entities;
using MangaPublishingSystem.Application.DTOs.Tasks;
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
        private readonly INotificationPublisher _notificationPublisher;

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
            INotificationPublisher notificationPublisher) 
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
            _notificationPublisher = notificationPublisher;
        }

        public async Task<TasksDto> CreateTaskAsync(int mangakaId, CreateTaskDto createDto)
        {
            var region = await _regionRepository.GetByIdAsync(createDto.RegionId);
            if (region == null)
            {
                throw new NotFoundException("Phân vùng vẽ không tồn tại trên hệ thống.");
            }

            var task = new Tasks
            {
                MangakaId = mangakaId,
                RegionId = createDto.RegionId,
                AssistantId = createDto.AssistantId,
                Description = createDto.Description,
                PaymentAmount = createDto.PaymentAmount,
                Deadline = createDto.Deadline,
                ZIndex_Order = createDto.ZIndex_Order,
                Status = "Draft",
                ExtensionStatus = "None"
            };

            await _tasksRepository.AddAsync(task);
            await _unitOfWork.SaveChangesAsync(); // Lưu để sinh TaskId tự động

            // Gọi khóa quỹ ví Escrow thù lao của Mangaka
            await _walletService.LockFundsAsync(mangakaId, task.PaymentAmount, task.Id);

            // Chuyển sang trạng thái Pending sau khi đã khóa quỹ an toàn
            task.Status = "Pending";
            _tasksRepository.Update(task);

            // Gửi thông báo đến trợ lý nếu đã được chỉ định
            if (task.AssistantId.HasValue)
            {
                var notif = new Notification
                {
                    UserId = task.AssistantId.Value,
                    Content = $"Bạn có lời mời nhận nhiệm vụ vẽ từ tác giả thù lao {task.PaymentAmount:N0} VND. Hạn chót: {task.Deadline:yyyy-MM-dd HH:mm}.",
                    Type = "Task_Assigned",
                    IsRead = false
                };
                await _notificationRepository.AddAsync(notif);
                await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notif.Content, notif.Type);
            }

            await _unitOfWork.SaveChangesAsync();
            return task.ToDto();
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
                await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notif.Content, notif.Type);

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
                await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notif.Content, notif.Type);
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
                    await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notif.Content, notif.Type);
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
                    await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notif.Content, notif.Type);
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
                await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notif.Content, notif.Type);

                // Cập nhật lại chỉ số Assistant
                await UpdateAssistantProfileMetricsAsync(task.AssistantId.Value);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<TasksDto>> GetTasksByMangakaIdAsync(int mangakaId)
        {
            var list = await _tasksRepository.FindAsync(t => t.MangakaId == mangakaId);
            return list.ToDtoList();
        }

        public async Task<IEnumerable<TasksDto>> GetTasksByAssistantIdAsync(int assistantId)
        {
            var list = await _tasksRepository.FindAsync(t => t.AssistantId == assistantId);
            return list.ToDtoList();
        }

        public async Task<IEnumerable<TaskVersionDto>> GetTaskVersionsAsync(int taskId)
        {
            var list = await _taskVersionRepository.FindAsync(v => v.TaskId == taskId);
            return list.ToDtoList();
        }

        public async Task<byte[]> GetCompositedPageAsync(int pageId)
        {
            var page = await _pageRepository.GetByIdAsync(pageId);
            if (page == null)
            {
                throw new NotFoundException("Trang truyện không tồn tại.");
            }

            var regions = await _regionRepository.FindAsync(r => r.PageId == pageId);
            var regionIds = regions.Select(r => r.Id).ToList();

            var tasks = await _tasksRepository.FindAsync(t => regionIds.Contains(t.RegionId) && t.Status == "Approved");
            var taskList = tasks.ToList();

            var layers = new List<(string overlayUrl, int zIndex)>();
            foreach (var task in taskList)
            {
                var versions = await _taskVersionRepository.FindAsync(v => v.TaskId == task.Id && v.Status == "Approved");
                var approvedVer = versions.FirstOrDefault();
                if (approvedVer != null)
                {
                    layers.Add((approvedVer.SubmittedFileUrl, task.ZIndex_Order));
                }
            }

            // Sắp xếp các lớp vẽ đè theo thứ tự Z-Index tăng dần
            var sortedLayers = layers.OrderBy(l => l.zIndex).ToList();

            return await _imageCompositor.CompositeLayersAsync(page.BaseLayerUrl ?? page.RawImageUrl, sortedLayers);
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
    }
}