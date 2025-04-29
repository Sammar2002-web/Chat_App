using ChatApp.ApplicationDbContext;
using ChatApp.Handlers;
using ChatApp.HubContext;
using ChatApp.Interfaces;
using ChatApp.Models;
using ChatApp.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace ChatApp.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAppLogsRepository _LogsRepository;
        private readonly IHubContext<ChatHub, IChatHub> _hubContext;

        public MessageRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor, IAppLogsRepository logsRepository, IHubContext<ChatHub, IChatHub> messageHub)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
            _LogsRepository = logsRepository;
            _hubContext = messageHub;
        }

        public async Task<BaseResult> CreatePrivateMessage(Message message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);

                var utcNow = DateTime.UtcNow;
                var localZone = TimeZoneInfo.FindSystemTimeZoneById("PST");
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);

                message.Timestamp = localTime;


                var currentUser = _contextAccessor.HttpContext?.User?.FindFirst(c => c.Type.Contains("Email"))?.Value;

                if (currentUser == null)
                {
                    return new BaseResult
                    {
                        IsError = true,
                        Message = "No NameIdentifier claim found",
                        Data = null
                    };
                }

                message.CreatedBy = currentUser;

                var data = await _dbContext.messages.AddAsync(message);
                await _dbContext.SaveChangesAsync();

                var msg = new SignalRMessage()
                {
                    Id = message.Id,
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    Message = message.Content,
                    Timestamp = message.Timestamp
                };
                var signalRMsg = _hubContext.Clients.All.SendPrivateMessage(msg.SenderId.ToString(), msg.Message, msg.ReceiverId.ToString());

                Debug.WriteLine($"SignalR message sent: {signalRMsg}");

                return new BaseResult
                {
                    Data = new
                    {
                        message.Id,
                        Timestamp = message.Timestamp.ToString("MM/dd/yyyy hh:mm:ss tt")
                    },
                    IsError = false,
                    Code = System.Net.HttpStatusCode.OK,
                    Message = "Message Sent Successfully"
                };
            }

            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BaseResult> Delete(int id)
        {
            try
            {
                var msg = await _dbContext.messages.FirstOrDefaultAsync(c => c.Id == id);

                if (msg == null)
                {
                    return new BaseResult
                    {
                        IsError = true,
                        Code = System.Net.HttpStatusCode.NotFound,
                        Message = "Message not found"
                    };
                }

                _dbContext.messages.Remove(msg);
                await _dbContext.SaveChangesAsync();

                return new BaseResult
                {
                    IsError = false,
                    Code = System.Net.HttpStatusCode.OK,
                    Message = "Message deleted successfully"
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BaseResult> Update(Message msg)
        {
            try
            {
                var oldMsg = await _dbContext.messages.FirstOrDefaultAsync(c => c.Id == msg.Id);

                if (oldMsg == null)
                {
                    return new BaseResult
                    {
                        IsError = true,
                        Code = System.Net.HttpStatusCode.NotFound,
                        Message = "Message not found"
                    };
                }

                oldMsg.Content = msg.Content;
                oldMsg.Timestamp = msg.Timestamp;
                await _dbContext.SaveChangesAsync();

                var updatedeMsgDto = new
                {
                    oldMsg.Id,
                    oldMsg.Content,
                    oldMsg.SenderId,
                    oldMsg.ReceiverId,
                    oldMsg.Timestamp,
                    oldMsg.CreatedBy
                };

                return new BaseResult
                {
                    IsError = false,
                    Code = System.Net.HttpStatusCode.OK,
                    Data = oldMsg,
                    Message = "Message updated successfully"
                };

            }
            catch(Exception)
            {
                return new BaseResult
                {
                    IsError = true,
                    Message = $"An error occurred while updating the user",
                    Data = null
                };
            }
        }

        public async Task<BaseResult> GetMessagesForUser(int userId)
        {
            try
            {
                var data = await _dbContext.messages.Where(m => m.ReceiverId == userId || m.SenderId == userId).ToListAsync();
                if (data == null || data.Count == 0)
                {
                    return new BaseResult
                    {
                        IsError = false,
                        Code = System.Net.HttpStatusCode.NotFound,
                        Message = "No messages found for this user"
                    };
                }

                return new BaseResult
                {
                    Data = data,
                    Code = System.Net.HttpStatusCode.OK,
                    IsError = false
                };
            }
            catch (Exception ex)
            {
                return new BaseResult
                {
                    IsError = true,
                    Message = $"An error occurred while retrieving messages: {ex.Message}",
                    Data = null
                };
            }
        }

    }
}
