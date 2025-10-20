using ChatApp.Handlers;
using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IMessageRepository
    {
        Task<BaseResult> GetMessagesForUser(int userId, int recipientId);
        Task<BaseResult> Delete(int id);
        Task<BaseResult> Update(Message msg);
        Task<BaseResult> CreatePrivateMessage(Message message);
    }
}
