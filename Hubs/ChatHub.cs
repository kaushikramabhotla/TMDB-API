using Microsoft.AspNetCore.SignalR;

namespace TMDB_API.Hubs
{
    public class ChatHub : Hub
    {
        // Called when a user connects — they join a group named by their userId
        // so we can target them specifically
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

        public async Task SendMessageToFriend(
            string receiverId,
            string senderId,
            string senderName,
            string message)
        {
            Console.WriteLine( $"SENDING MESSAGE");

            Console.WriteLine($"Receiver Group: {receiverId}");

            Console.WriteLine( $"Sender: {senderName}");

            Console.WriteLine($"Message: {message}");

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