using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Application.IServices;
using MangaPublishingSystem.Presentation.Hubs;

namespace MangaPublishingSystem.Presentation.Services
{
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationPublisher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishNotificationAsync(int userId, string content, string type)
        {
            // Gửi thông báo dạng cũ để tránh lỗi tương thích
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", content, type);
        }

        public async Task PublishNotificationPayloadAsync(int userId, NotificationPayload payload)
        {
            // Gửi event NewNotification cho Frontend lắng nghe
            await _hubContext.Clients.User(userId.ToString()).SendAsync("NewNotification", payload);
        }

        public async Task PublishTaskStatusChangedAsync(int userId, TaskStatusChangedPayload payload)
        {
            // Gửi event TaskStatusChanged cho Frontend lắng nghe
            await _hubContext.Clients.User(userId.ToString()).SendAsync("TaskStatusChanged", payload);
        }

        public async Task PublishWalletUpdatedAsync(int userId, WalletUpdatedPayload payload)
        {
            // Gửi event WalletUpdated cho Frontend lắng nghe
            await _hubContext.Clients.User(userId.ToString()).SendAsync("WalletUpdated", payload);
        }

        public async Task PublishUnreadCountUpdatedAsync(int userId, int count)
        {
            // Gửi event UnreadCountUpdated cho Frontend lắng nghe
            await _hubContext.Clients.User(userId.ToString()).SendAsync("UnreadCountUpdated", new { Count = count });
        }

        public async Task PublishBoardDataChangedAsync()
        {
            // Gửi event BoardDataChanged cho tất cả các Client đang kết nối
            await _hubContext.Clients.All.SendAsync("BoardDataChanged");
        }

        public async Task PublishTaskQueueChangedAsync(TaskStatusChangedPayload payload)
        {
            await _hubContext.Clients.All.SendAsync("TaskStatusChanged", payload);
        }
    }
}
