using ChatApp.Handlers;
using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IMessageRepository
    {
        Task<BaseResult> Create(Message message);
        Task<BaseResult> Delete(int id);
        Task<BaseResult> Update(int id);
        Task<BaseResult> GetMessages(int id);
    }
}
