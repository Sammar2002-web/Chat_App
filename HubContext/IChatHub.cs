using ChatApp.Models;

namespace ChatApp.HubContext
{
    public interface IChatHub
    {
        Task ReceiveMessage(Message message);
    }
}
