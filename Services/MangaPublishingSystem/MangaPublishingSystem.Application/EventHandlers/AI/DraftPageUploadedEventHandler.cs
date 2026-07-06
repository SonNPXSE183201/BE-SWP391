using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using MangaPublishingSystem.Application.Events.AI;
using MangaPublishingSystem.Application.IServices.AI;

namespace MangaPublishingSystem.Application.EventHandlers.AI
{
    public class DraftPageUploadedEventHandler : INotificationHandler<DraftPageUploadedEvent>
    {
        private readonly IAiVisionClient _aiVisionClient;
        private readonly ILogger<DraftPageUploadedEventHandler> _logger;

        public DraftPageUploadedEventHandler(
            IAiVisionClient aiVisionClient,
            ILogger<DraftPageUploadedEventHandler> logger)
        {
            _aiVisionClient = aiVisionClient;
            _logger = logger;
        }

        public async Task Handle(DraftPageUploadedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Nhận sự kiện DraftPageUploadedEvent cho PageId {PageId}. Bắt đầu gửi ảnh sang AI Module.", notification.PageId);

            var result = await _aiVisionClient.SegmentMangaPanelsAsync(notification.ImageUrl, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("AI phân vùng thành công cho PageId {PageId}. Số lượng panel: {Count}", notification.PageId, result.Panels.Count);
                // TODO: Inject IUnitOfWork and save regions to Database here.
            }
            else
            {
                _logger.LogWarning("AI phân vùng thất bại cho PageId {PageId}.", notification.PageId);
            }
        }
    }
}
