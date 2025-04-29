using ChatApp.Interfaces;
using ChatApp.Models;
using ChatApp.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Controllers
{
    //[Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messagRepository;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessageController(IMessageRepository messagRepository, IHubContext<ChatHub> hubContext)
        {
            _messagRepository = messagRepository;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Message message)
        {
            await _messagRepository.CreatePrivateMessage(message);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int id)
        {
            var data = await _messagRepository.GetMessagesForUser(id);
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
