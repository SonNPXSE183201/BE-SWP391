using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                        var taskAutomationService = scope.ServiceProvider.GetRequiredService<ITaskAutomationService>();

                        await taskAutomationService.AutoRefundOverdueTasksAsync();
                        await taskAutomationService.AutoApproveSubmittedTasksAsync();
                        await taskAutomationService.CleanExpiredRefreshTokensAsync();
                        await taskAutomationService.AutoResolveExpiredBoardVotesAsync();
                        await taskAutomationService.AutoSettleGracePeriodTasksAsync();
                    }
                    _logger.LogInformation("Quét tự động hóa nhiệm vụ hoàn tất.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Có lỗi xảy ra trong chu kỳ quét tự động hóa nhiệm vụ.");
                }

                // Chạy mỗi 1 phút để dễ test
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
