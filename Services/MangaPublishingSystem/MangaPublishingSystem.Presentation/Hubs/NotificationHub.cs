using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MangaPublishingSystem.Presentation.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier; // Will be available if JWT auth is integrated
            _logger.LogInformation("Client connected: {ConnectionId}, User: {UserId}", connectionId, userId);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Client disconnected: {ConnectionId}, Reason: {Message}", connectionId, exception?.Message ?? "No exception");
            
            await base.OnDisconnectedAsync(exception);
        }

        // Example method: Clients can call this to join their own personal group or a role group
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} joined group: {GroupName}", Context.ConnectionId, groupName);
        }
    }
}
