using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Notifications;

namespace MangaPublishingSystem.Application.IServices
{
    public interface INotificationPublisher
    {
        Task PublishNotificationAsync(int userId, string content, string type);
        Task PublishNotificationPayloadAsync(int userId, NotificationPayload payload);
        Task PublishTaskStatusChangedAsync(int userId, TaskStatusChangedPayload payload);
        Task PublishWalletUpdatedAsync(int userId, WalletUpdatedPayload payload);
        Task PublishUnreadCountUpdatedAsync(int userId, int count);
        Task PublishBoardDataChangedAsync();
        /// <summary>Bảng việc làm công khai — broadcast tới mọi client đang kết nối.</summary>
        Task PublishTaskQueueChangedAsync(TaskStatusChangedPayload payload);
    }
}
