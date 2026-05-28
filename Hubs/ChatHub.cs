using Microsoft.AspNetCore.SignalR;
using TMDB_API.Models.Mongo;
using TMDB_API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TMDB_API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        // Called when a user connects — they join a group named by their userId
        // so we can target them specifically

        private readonly MessageService _messageService;
        private static HashSet<string> OnlineUsers = new();

        public ChatHub(MessageService messageService)
        {
            _messageService = messageService;
        }
        public async Task JoinUserGroup(string userId)
        {
            Console.WriteLine(
                $"JOINING GROUP: {userId}"
            );

            Console.WriteLine(
                $"CONNECTION ID: {Context.ConnectionId}"
            );

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                userId
            );

            Console.WriteLine(
                "GROUP JOIN SUCCESS"
            );
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                OnlineUsers.Add(userId);
                await Clients.All.SendAsync("UserOnline",userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                OnlineUsers.Remove(userId);
                await Clients.All.SendAsync("UserOffline",userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToFriend(
            string receiverId,
            string message)
        {

            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var senderName = Context.User?.FindFirst("username")?.Value;

            //Console.WriteLine( $"SENDING MESSAGE");

            //Console.WriteLine($"Receiver Group: {receiverId}");

            //Console.WriteLine( $"Sender: {senderName}");

            //Console.WriteLine($"Message: {message}");

            try
            {
                await _messageService.SaveMessage(
                        new ChatMessage
                        {
                            SenderId = senderId,
                            ReceiverId = receiverId,
                            SenderName = senderName,
                            Message = message,
                            SentAt = DateTime.UtcNow,
                            IsRead = false
                        }
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MONGO SAVE ERROR: {ex}");
            }

            await Clients.Group(receiverId)
                .SendAsync(
                    "ReceiveDirectMessage",
                    senderId,
                    senderName,
                    message
                );


            Console.WriteLine("MESSAGE SENT TO SIGNALR GROUP");
        }
    }
}