using ChatApp.HubContext;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.RecieveMessage($"{Context.ConnectionId} has joined");
        }
        public async Task SendMessage(string message)
        {
            await Clients.All.RecieveMessage($"{Context.ConnectionId} : {message}");
        }
    }
}
