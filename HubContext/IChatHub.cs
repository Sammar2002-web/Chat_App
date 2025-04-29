using ChatApp.Models;

namespace ChatApp.HubContext
{
    public interface IChatHub
    {
        Task ReceiveMessage(string message);
        Task SendPrivateMessage(string userId, string message, string recieverId);
    }
}
