using ChatApp.HubContext;
using ChatApp.Interfaces;
using ChatApp.Models;
using ChatApp.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messagRepository;
        private readonly IHubContext<ChatHub, IChatHub> _hubContext;

        public MessageController(IMessageRepository messagRepository, IHubContext<ChatHub, IChatHub> hubContext)
        {
            _messagRepository = messagRepository;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Message message)
        {
            var result = await _messagRepository.CreatePrivateMessage(message);
            await _hubContext.Clients.User(message.ReceiverId.ToString()).ReceiveMessage(message);
            await _hubContext.Clients.User(message.SenderId.ToString()).ReceiveMessage(message);
            return Ok(result);
        }

        [HttpGet("{userId}/{recipientId}")]
        public async Task<IActionResult> GetMessages(int userId, int recipientId)
        {
            var data = await _messagRepository.GetMessagesForUser(userId, recipientId);
            return Ok(data);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _messagRepository.Delete(id);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Message msg)
        {
            var data = await _messagRepository.Update(msg);
            return Ok(data);
        }
    }
}
