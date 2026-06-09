using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification = MangaPublishingSystem.Domain.Entities.Notification;
using MangaPublishingSystem.Domain.Enums;
using MangaPublishingSystem.Application.IRepositories;
using MangaPublishingSystem.Application.IServices;

namespace MangaPublishingSystem.Presentation.Services
{
    public class TaskAutomationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskAutomationBackgroundService> _logger;

        public TaskAutomationBackgroundService(IServiceProvider serviceProvider, ILogger<TaskAutomationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ tự động hóa nhiệm vụ MCWPMS đã khởi chạy.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Bắt đầu chu kỳ quét tự động hóa nhiệm vụ...");
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tasksRepository = scope.ServiceProvider.GetRequiredService<ITasksRepository>();
                        var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
                        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                        var notificationPublisher = scope.ServiceProvider.GetRequiredService<INotificationPublisher>();
                        var taskVersionRepository = scope.ServiceProvider.GetRequiredService<ITaskVersionRepository>();
                        var assistantProfileRepository = scope.ServiceProvider.GetRequiredService<IAssistantProfileRepository>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

                        await AutoRefundOverdueTasksAsync(tasksRepository, walletService, notificationRepository, notificationPublisher, taskVersionRepository, assistantProfileRepository, unitOfWork);
                        await AutoApproveSubmittedTasksAsync(tasksRepository, walletService, userRepository, notificationRepository, notificationPublisher, taskVersionRepository, assistantProfileRepository, unitOfWork);
                        await CleanExpiredRefreshTokensAsync(refreshTokenRepository, unitOfWork);
                    }
                    _logger.LogInformation("Quét tự động hóa nhiệm vụ hoàn tất.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Có lỗi xảy ra trong chu kỳ quét tự động hóa nhiệm vụ.");
                }

                // Chạy mỗi 1 giờ (có thể cấu hình nhanh hơn để thử nghiệm)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task AutoRefundOverdueTasksAsync(
            ITasksRepository tasksRepository,
            IWalletService walletService,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            ITaskVersionRepository taskVersionRepository,
            IAssistantProfileRepository assistantProfileRepository,
            IUnitOfWork unitOfWork)
        {
            // T03a: Quá hạn chót 3 ngày mà không tải bài nộp (Trạng thái In_Progress hoặc Revision)
            var now = DateTime.UtcNow;
            var overdueTasks = await tasksRepository.FindAsync(t => 
                (t.Status == "In_Progress" || t.Status == "Revision") && 
                t.Deadline.AddDays(3) < now);

            foreach (var task in overdueTasks)
            {
                _logger.LogInformation("Bắt đầu tự động hoàn tiền/hủy task quá hạn: TaskId {TaskId}", task.Id);
                try
                {
                    // Trả lại quỹ ký quỹ về ví Mangaka
                    await walletService.ReleaseFundsAsync(task.Id, isApproved: false);

                    task.Status = "Cancelled";
                    tasksRepository.Update(task);

                    // Thông báo Mangaka
                    var notifMangaka = new Notification
                    {
                        UserId = task.MangakaId,
                        Content = $"Nhiệm vụ '{task.Description}' đã bị hệ thống tự động hủy và hoàn lại tiền ký quỹ do Assistant trễ hạn nộp bài quá 3 ngày.",
                        Type = "Task_AutoRefund_Mangaka",
                        IsRead = false
                    };
                    await notificationRepository.AddAsync(notifMangaka);
                    await notificationPublisher.PublishNotificationAsync(task.MangakaId, notifMangaka.Content, notifMangaka.Type);

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
                        await notificationRepository.AddAsync(notifAssistant);
                        await notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notifAssistant.Content, notifAssistant.Type);

                        // Cập nhật chỉ số trợ lý
                        await UpdateAssistantStats(task.AssistantId.Value, tasksRepository, taskVersionRepository, assistantProfileRepository);
                    }

                    await unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tự động hủy/hoàn tiền task quá hạn: TaskId {TaskId}", task.Id);
                }
            }
        }

        private async Task AutoApproveSubmittedTasksAsync(
            ITasksRepository tasksRepository,
            IWalletService walletService,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            INotificationPublisher notificationPublisher,
            ITaskVersionRepository taskVersionRepository,
            IAssistantProfileRepository assistantProfileRepository,
            IUnitOfWork unitOfWork)
        {
            // T04: Tự động duyệt sau 3 ngày nộp bài nếu tác giả không phản hồi
            var now = DateTime.UtcNow;
            var submittedTasks = await tasksRepository.FindAsync(t => t.Status == "Submitted");

            foreach (var task in submittedTasks)
            {
                try
                {
                    // Kiểm tra Mangaka có đang nghỉ phép (OnLeave) hay không
                    var mangaka = await userRepository.GetByIdAsync(task.MangakaId);
                    if (mangaka != null && mangaka.Skills == "OnLeave")
                    {
                        // Tạm đóng băng đếm ngược
                        continue;
                    }

                    var versions = await taskVersionRepository.FindAsync(v => v.TaskId == task.Id);
                    var latestVersion = versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
                    if (latestVersion == null) continue;

                    var timeSubmitted = latestVersion.SubmittedAt;

                    // Nếu đã quá 3 ngày nộp bài
                    if (timeSubmitted.AddDays(3) < now)
                    {
                        _logger.LogInformation("Tự động duyệt bài nộp: TaskId {TaskId} sau 3 ngày không phản hồi.", task.Id);
                        
                        await walletService.ReleaseFundsAsync(task.Id, isApproved: true);

                        task.Status = "Approved";
                        task.FeedbackComment = "Tự động duyệt bởi hệ thống.";
                        tasksRepository.Update(task);

                        latestVersion.Status = "Approved";
                        taskVersionRepository.Update(latestVersion);

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
                            await notificationRepository.AddAsync(notifAssistant);
                            await notificationPublisher.PublishNotificationAsync(task.AssistantId.Value, notifAssistant.Content, notifAssistant.Type);

                            await UpdateAssistantStats(task.AssistantId.Value, tasksRepository, taskVersionRepository, assistantProfileRepository);
                        }

                        // Thông báo Mangaka
                        var notifMangaka = new Notification
                        {
                            UserId = task.MangakaId,
                            Content = $"Hệ thống đã tự động nghiệm thu và giải ngân nhiệm vụ '{task.Description}' do bạn không phản hồi sau 3 ngày nộp bài.",
                            Type = "Task_AutoApprove_Mangaka",
                            IsRead = false
                        };
                        await notificationRepository.AddAsync(notifMangaka);
                        await notificationPublisher.PublishNotificationAsync(task.MangakaId, notifMangaka.Content, notifMangaka.Type);

                        await unitOfWork.SaveChangesAsync();
                    }
                    // Cảnh báo vào ngày thứ 2 (quá 2 ngày)
                    else if (timeSubmitted.AddDays(2) < now)
                    {
                        // Kiểm tra xem đã gửi cảnh báo ngày thứ 2 chưa
                        var existingAlerts = await notificationRepository.FindAsync(n => 
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
                            await notificationRepository.AddAsync(warningNotif);
                            await notificationPublisher.PublishNotificationAsync(task.MangakaId, warningNotif.Content, warningNotif.Type);
                            await unitOfWork.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tự động duyệt task: TaskId {TaskId}", task.Id);
                }
            }
        }

        private async Task UpdateAssistantStats(
            int assistantId,
            ITasksRepository tasksRepository,
            ITaskVersionRepository taskVersionRepository,
            IAssistantProfileRepository assistantProfileRepository)
        {
            var profileList = await assistantProfileRepository.FindAsync(p => p.AssistantId == assistantId);
            var profile = profileList.FirstOrDefault();
            if (profile == null) return;

            var allTasks = await tasksRepository.FindAsync(t => t.AssistantId == assistantId);
            var tasksList = allTasks.ToList();

            var approvedTasks = tasksList.Where(t => t.Status == "Approved").ToList();
            profile.TotalCompletedTasks = approvedTasks.Count;

            if (approvedTasks.Count > 0)
            {
                int onTimeCount = 0;
                foreach (var task in approvedTasks)
                {
                    var versions = await taskVersionRepository.FindAsync(v => v.TaskId == task.Id && v.Status == "Approved");
                    var approvedVer = versions.FirstOrDefault();
                    if (approvedVer != null && approvedVer.SubmittedAt <= task.Deadline)
                    {
                        onTimeCount++;
                    }
                    else if (approvedVer == null)
                    {
                        var allVer = await taskVersionRepository.FindAsync(v => v.TaskId == task.Id);
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

            assistantProfileRepository.Update(profile);
        }

        private async Task CleanExpiredRefreshTokensAsync(
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork)
        {
            _logger.LogInformation("Bắt đầu dọn dẹp các RefreshToken đã hết hạn hoặc bị thu hồi...");
            try
            {
                var now = DateTime.UtcNow;
                var thresholdDate = now.AddDays(-30);
                var expiredOrRevokedTokens = await refreshTokenRepository.FindAsync(t => 
                    t.ExpiresAt < now || 
                    (t.IsRevoked && t.CreateAt < thresholdDate));
                
                int count = 0;
                foreach (var token in expiredOrRevokedTokens)
                 {
                     refreshTokenRepository.Delete(token);
                     count++;
                 }

                 if (count > 0)
                 {
                     await unitOfWork.SaveChangesAsync();
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
    }
}
