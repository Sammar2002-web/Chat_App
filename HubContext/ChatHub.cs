using ChatApp.ApplicationDbContext;
using ChatApp.Handlers;
using ChatApp.HubContext;
using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
        private readonly AppDbContext _dbContext;

        public ChatHub(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendGroupMessage(Message message)
        {
            if (message.GroupId == null)
            {
                throw new ArgumentNullException(nameof(message.GroupId), "GroupId cannot be null for group messages.");
            }

            await Clients.Group(message.GroupId.ToString()!)
                .ReceiveMessage(message);
        }

        public async Task SendBroadcastMessage(Message message)
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

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User connected: {userId} (ConnId: {Context.ConnectionId})");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User disconnected: {userId} (ConnId: {Context.ConnectionId})");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
