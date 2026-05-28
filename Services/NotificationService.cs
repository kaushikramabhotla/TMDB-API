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
            try
            {
                var subscriber = _redis.GetSubscriber();

                await subscriber.SubscribeAsync("notifications",
                    async (channel, message) =>
                    {
                        var payload =
                            JsonSerializer.Deserialize<NotificationPayload>(message.ToString());

                        if (payload != null)
                        {
                            await _hubContext
                                .Clients
                                .Group(payload.TargetUserId)
                                .SendAsync(
                                    "ReceiveNotification",
                                    payload.Type,
                                    payload.Message
                                );
                        }
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Redis subscription failed: {ex.Message}"
                );
            }
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