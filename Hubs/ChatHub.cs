using Microsoft.AspNetCore.SignalR;

namespace TMDB_API.Hubs
{
    public class ChatHub : Hub
    {
        public async Task FriendAccepted(
            string requestId)
        {
            await Clients.All.SendAsync(
                "FriendAdded",
                requestId
            );
        }
    }
}
