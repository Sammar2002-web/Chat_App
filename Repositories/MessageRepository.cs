using ChatApp.ApplicationDbContext;
using ChatApp.Handlers;
using ChatApp.Interfaces;
using ChatApp.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ChatApp.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;

        public MessageRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
        }

        public Task<BaseResult> GetMessages(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult> Create(Message message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                //var date = message.Timestamp.ToString("MM/dd/yyyy HH:mm:ss");

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

                currentUser = message.CreatedBy;

                var data = await _dbContext.messages.AddAsync(message);
                await _dbContext.SaveChangesAsync();

                return new BaseResult
                {
                    Data = data.Entity,
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

        public Task<BaseResult> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResult> Update(int id)
        {
            throw new NotImplementedException();
        }
    }
}
