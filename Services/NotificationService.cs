using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;
using TMDB_API.Hubs;

namespace TMDB_API.Services
{
    // This runs in the background constantly, listening to Redis
    public class NotificationService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IHubContext<ChatHub> _hubContext;

        public NotificationService(IConnectionMultiplexer redis, IHubContext<ChatHub> hubContext)
        {
            _redis = redis;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber(); 

            // Subscribe to the "notifications" channel in Redis
            // Whenever something is published here, this fires
            await subscriber.SubscribeAsync("notifications", async (channel, value) =>
            {
                var notification = JsonSerializer.Deserialize<NotificationPayload>(value!.ToString()!);
                if (notification == null) return;

                // Push to the specific user's SignalR group
                await _hubContext.Clients
                    .Group(notification.TargetUserId)
                    .SendAsync("ReceiveNotification", notification.Type, notification.Message);
            });

            // Keep alive until app shuts down
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    public class NotificationPayload
    {
        public string TargetUserId { get; set; }  // who receives it
        public string Type { get; set; }           // "accepted" or "rejected"
        public string Message { get; set; }

        public DateTime Time { get; set; }
    }
}