using ChatApp.ApplicationDbContext;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            var data = await _userRepository.Create(user);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var data = await _userRepository.Login(login);
            return Ok(data);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var data = await _userRepository.GetAll();
            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _userRepository.GetById(id);
            return Ok(data);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] User user)
        {
            var data = await _userRepository.Update(user);
            return Ok(data);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _userRepository.Delete(id);
            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UserDto userDto)
        {
            var result = await _userRepository.UpdateProfile(id, userDto);
            return Ok(result);
        }
    }
}
