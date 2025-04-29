using ChatApp.Handlers;
using ChatApp.Models;

namespace ChatApp.Interfaces
{
    public interface IAppLogsRepository
    {
        Task<BaseResult> GetLogs();
        Task<BaseResult> AddLogs(AppLogs logs);
    }
}
