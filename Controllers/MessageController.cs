using ChatApp.Handlers;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    //[Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messagRepository;

        public MessageController(IMessageRepository messagRepository)
        {
            _messagRepository = messagRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Message message)
        {
            var data = await _messagRepository.Create(message);
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int id)
        {
            var data = await _messagRepository.GetMessages(id);
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
