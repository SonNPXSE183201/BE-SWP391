using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;
using Notification = MangaPublishingSystem.Domain.Entities.Notification;

namespace MangaPublishingSystem.Application.Services
{
    public class TaskAutomationService : ITaskAutomationService
    {
        private readonly ITasksRepository _tasksRepository;
        private readonly IWalletService _walletService;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly ITaskVersionRepository _taskVersionRepository;
        private readonly IAssistantProfileRepository _assistantProfileRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TaskAutomationService> _logger;

        public TaskAutomationService(
            ITasksRepository tasksRepository,
            IWalletService walletService,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            ITaskVersionRepository taskVersionRepository,
            IAssistantProfileRepository assistantProfileRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            ILogger<TaskAutomationService> logger)
        {
            _tasksRepository = tasksRepository;
            _walletService = walletService;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationPublisher = notificationPublisher;
            _taskVersionRepository = taskVersionRepository;
            _assistantProfileRepository = assistantProfileRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task AutoRefundOverdueTasksAsync()
        {
            // T03a: Quá hạn chót 3 ngày mà không tải bài nộp (Trạng thái In_Progress hoặc Revision)
            var now = DateTime.UtcNow;
            var overdueTasks = await _tasksRepository.FindAsync(t => 
                (t.Status == "In_Progress" || t.Status == "Revision") && 
                t.Deadline.AddDays(3) < now);

            foreach (var task in overdueTasks)
            {
                _logger.LogInformation("Bắt đầu tự động hoàn tiền/hủy task quá hạn: TaskId {TaskId}", task.Id);
                try
                {
                    // Trả lại quỹ ký quỹ về ví Mangaka
                    await _walletService.ReleaseFundsAsync(task.Id, isApproved: false);

                    task.Status = "Cancelled";
                    _tasksRepository.Update(task);

                    // Thông báo Mangaka
                    var notifMangaka = new Notification
                    {
                        UserId = task.MangakaId,
                        Content = $"Nhiệm vụ '{task.Description}' đã bị hệ thống tự động hủy và hoàn lại tiền ký quỹ do Assistant trễ hạn nộp bài quá 3 ngày.",
                        Type = "Task_AutoRefund_Mangaka",
                        IsRead = false
                    };
                    await _notificationRepository.AddAsync(notifMangaka);
                    await _notificationPublisher.PublishNotificationAsync(task.MangakaId, notifMangaka.Content, notifMangaka.Type);

                    // Thông báo Assistant
                    if (task.AssistantId.HasValue)
                    {
                        var notifAssistant = new Notification
                        {
                            UserId = task.AssistantId.Value,
                            Content = $"Nhiệm vụ '{task.Description}' đã bị tự động hủy do bạn trễ hạn nộp bài quá 3 ngày.",
                            Type = "Task_AutoRefund_Assistant",
                            IsRead = false
                        };
                        await _notificationRepository.AddAsync(notifAssistant);
                        await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notifAssistant.Content, notifAssistant.Type);

                        // Cập nhật chỉ số trợ lý
                        await UpdateAssistantStats(task.AssistantId.Value);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tự động hủy/hoàn tiền task quá hạn: TaskId {TaskId}", task.Id);
                }
            }
        }

        public async Task AutoApproveSubmittedTasksAsync()
        {
            // T04: Tự động duyệt sau 3 ngày nộp bài nếu tác giả không phản hồi
            var now = DateTime.UtcNow;
            var submittedTasks = await _tasksRepository.FindAsync(t => t.Status == "Submitted");

            foreach (var task in submittedTasks)
            {
                try
                {
                    // Kiểm tra Mangaka có đang nghỉ phép (IsOnLeave) hay không
                    var mangaka = await _userRepository.GetByIdAsync(task.MangakaId);
                    if (mangaka != null && mangaka.IsOnLeave)
                    {
                        // Tạm đóng băng đếm ngược
                        continue;
                    }

                    var versions = await _taskVersionRepository.FindAsync(v => v.TaskId == task.Id);
                    var latestVersion = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
                    if (latestVersion == null) continue;

                    var timeSubmitted = latestVersion.SubmittedAt;

                    // Nếu đã quá 3 ngày nộp bài
                    if (timeSubmitted.AddDays(3) < now)
                    {
                        _logger.LogInformation("Tự động duyệt bài nộp: TaskId {TaskId} sau 3 ngày không phản hồi.", task.Id);
                        
                        await _walletService.ReleaseFundsAsync(task.Id, isApproved: true);

                        task.Status = "Approved";
                        task.FeedbackComment = "Tự động duyệt bởi hệ thống.";
                        _tasksRepository.Update(task);

                        latestVersion.Status = "Approved";
                        _taskVersionRepository.Update(latestVersion);

                        // Thông báo Assistant
                        if (task.AssistantId.HasValue)
                        {
                            var notifAssistant = new Notification
                            {
                                UserId = task.AssistantId.Value,
                                Content = $"Bài vẽ của nhiệm vụ '{task.Description}' đã được hệ thống tự động nghiệm thu sau 3 ngày. Thù lao đã chuyển về ví của bạn.",
                                Type = "Task_AutoApprove_Assistant",
                                IsRead = false
                            };
                            await _notificationRepository.AddAsync(notifAssistant);
                            await _notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notifAssistant.Content, notifAssistant.Type);

                            await UpdateAssistantStats(task.AssistantId.Value);
                        }

                        // Thông báo Mangaka
                        var notifMangaka = new Notification
                        {
                            UserId = task.MangakaId,
                            Content = $"Hệ thống đã tự động nghiệm thu và giải ngân nhiệm vụ '{task.Description}' do bạn không phản hồi sau 3 ngày nộp bài.",
                            Type = "Task_AutoApprove_Mangaka",
                            IsRead = false
                        };
                        await _notificationRepository.AddAsync(notifMangaka);
                        await _notificationPublisher.PublishNotificationAsync(task.MangakaId, notifMangaka.Content, notifMangaka.Type);

                        await _unitOfWork.SaveChangesAsync();
                    }
                    // Cảnh báo vào ngày thứ 2 (quá 2 ngày)
                    else if (timeSubmitted.AddDays(2) < now)
                    {
                        // Kiểm tra xem đã gửi cảnh báo ngày thứ 2 chưa
                        var existingAlerts = await _notificationRepository.FindAsync(n => 
                            n.UserId == task.MangakaId && 
                            n.Type == "Task_AutoApprove_Warning" && 
                            n.Content.Contains(task.Id.ToString()));

                        if (!existingAlerts.Any())
                        {
                            _logger.LogInformation("Gửi cảnh báo ngày thứ 2 tự động duyệt cho Mangaka: TaskId {TaskId}", task.Id);
                            var warningNotif = new Notification
                            {
                                UserId = task.MangakaId,
                                Content = $"Cảnh báo: Nhiệm vụ vẽ ID {task.Id} ('{task.Description}') sẽ tự động được duyệt và giải ngân sau 24 giờ nữa nếu bạn không có phản hồi.",
                                Type = "Task_AutoApprove_Warning",
                                IsRead = false
                            };
                            await _notificationRepository.AddAsync(warningNotif);
                            await _notificationPublisher.PublishNotificationAsync(task.MangakaId, warningNotif.Content, warningNotif.Type);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tự động duyệt task: TaskId {TaskId}", task.Id);
                }
            }
        }

        public async Task CleanExpiredRefreshTokensAsync()
        {
            _logger.LogInformation("Bắt đầu dọn dẹp các RefreshToken đã hết hạn hoặc bị thu hồi...");
            try
            {
                var now = DateTime.UtcNow;
                var thresholdDate = now.AddDays(-30);
                var expiredOrRevokedTokens = await _refreshTokenRepository.FindAsync(t => 
                    t.ExpiresAt < now || 
                    (t.IsRevoked && t.CreateAt < thresholdDate));
                
                int count = 0;
                foreach (var token in expiredOrRevokedTokens)
                 {
                     _refreshTokenRepository.Delete(token);
                     count++;
                 }

                 if (count > 0)
                 {
                     await _unitOfWork.SaveChangesAsync();
                     _logger.LogInformation("Đã dọn dẹp thành công {Count} RefreshToken rác khỏi DB.", count);
                 }
                 else
                 {
                     _logger.LogInformation("Không có RefreshToken rác nào cần dọn dẹp.");
                 }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tự động dọn dẹp RefreshToken.");
            }
        }

        private async Task UpdateAssistantStats(int assistantId)
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
