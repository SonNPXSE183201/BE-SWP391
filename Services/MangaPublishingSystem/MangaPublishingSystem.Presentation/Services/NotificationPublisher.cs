using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
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
            // Gửi thông báo tới User thông qua connection của UserIdentifier
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", content, type);
        }
    }
}
