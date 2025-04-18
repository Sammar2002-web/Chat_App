using ChatApp.Handlers;
using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IMessageRepository
    {
        Task<BaseResult> GetMessagesForUser(int userId);
        Task<BaseResult> Delete(int id);
        Task<BaseResult> Update(Message msg);
        Task AddMessage(Message message);
        //Task<BaseResult> GetMessages(int id);
        //Task<BaseResult> Create(Message message);
    }
}
