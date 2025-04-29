using ChatApp.HubContext;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
        public async Task SendPrivateMessage(string userId, string message, string recieverId)
        {
            await Clients.User(userId).ReceiveMessage(message);
        }

        public async Task SendGroupMessage(string groupName, string message)
        {
            await Clients.Group(groupName).ReceiveMessage(message);
        }

        public async Task SendBroadcastMessage(string message)
        {
            await Clients.All.ReceiveMessage(message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
