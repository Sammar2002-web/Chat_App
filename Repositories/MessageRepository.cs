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
                if (message.ReceiverId == null)
                {
                    //message.ReceiverId = 2;
                    return new BaseResult
                    {
                        IsError = true,
                        Code = System.Net.HttpStatusCode.BadRequest,
                        Message = "ReceiverId is required."
                    };
                }

                try
                {
                    var utcNow = DateTime.UtcNow;
                    var localZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);

                    message.Timestamp = localTime;

                    await _dbContext.messages.AddAsync(message);
                    await _dbContext.SaveChangesAsync();

                    await _hubContext.Clients
                                     .User(message.ReceiverId.Value.ToString())
                                     .ReceiveMessage(message);

                    return new BaseResult
                    {
                        Data = new
                        {
                            message.Id,
                            Timestamp = message.Timestamp.ToString("dd/MM/yyyy hh:mm:ss tt")
                        },
                        IsError = false,
                        Code = System.Net.HttpStatusCode.OK,
                        Message = "Message Sent Successfully ✅"
                    };
                }
                catch (Exception ex)
                {
                    return new BaseResult
                    {
                        IsError = true,
                        Code = System.Net.HttpStatusCode.InternalServerError,
                        Message = $"Failed to send message: {ex.Message}"
                    };
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        //public async Task<BaseResult> CreatePrivateMessage(Message message)
        //{
        //    try
        //    {
        //        var utcNow = DateTime.UtcNow;
        //        var localZone = TimeZoneInfo.FindSystemTimeZoneById("PST");
        //        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);

        //        message.Timestamp = localTime;

        //        var data = await _dbContext.messages.AddAsync(message);
        //        await _dbContext.SaveChangesAsync();

        //        await _hubContext.Clients.User(message.ReceiverId.ToString()!).ReceiveMessage(message.Content);

        //        return new BaseResult
        //        {
        //            Data = new
        //            {
        //                message.Id,
        //                Timestamp = message.Timestamp.ToString("MM/dd/yyyy hh:mm:ss tt")
        //            },
        //            IsError = false,
        //            Code = System.Net.HttpStatusCode.OK,
        //            Message = "Message Sent Successfully"
        //        };
        //    }

        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

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
                    oldMsg.Timestamp
                };

                return new BaseResult
                {
                    IsError = false,
                    Code = System.Net.HttpStatusCode.OK,
                    Data = oldMsg,
                    Message = "Message updated successfully"
                };

            }
            catch (Exception)
            {
                return new BaseResult
                {
                    IsError = true,
                    Message = $"An error occurred while updating the user",
                    Data = null
                };
            }
        }

        public async Task<BaseResult> GetMessagesForUser(int userId, int recipientId)
        {
            try
            {
                var data = await _dbContext.messages
                    .Where(m => (m.SenderId == userId && m.ReceiverId == recipientId) || (m.SenderId == recipientId && m.ReceiverId == userId))
                    .ToListAsync();

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
