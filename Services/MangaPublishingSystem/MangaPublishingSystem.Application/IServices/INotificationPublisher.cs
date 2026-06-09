using System.Threading.Tasks;

namespace MangaPublishingSystem.Application.IServices
{
    public interface INotificationPublisher
    {
        Task PublishNotificationAsync(int userId, string content, string type);
    }
}
