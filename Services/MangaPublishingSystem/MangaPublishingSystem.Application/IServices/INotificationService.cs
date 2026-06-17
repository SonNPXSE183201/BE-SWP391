using System.Collections.Generic;
using System.Threading.Tasks;
using MangaPublishingSystem.Application.DTOs.Notifications;
using MangaPublishingSystem.Domain.Entities;

namespace MangaPublishingSystem.Application.IServices
{
    public interface INotificationService : IGenericService<Notification>
    {
        Task<IEnumerable<NotificationDto>> GetNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        System.Threading.Tasks.Task MarkAsReadAsync(int notificationId, int userId);
        System.Threading.Tasks.Task MarkAllAsReadAsync(int userId);
    }
}