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
        private readonly IUserRepository _repo;

        public UsersController(IUserRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            var data = await _repo.AddUser(user);
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var data = await _repo.Login(login);
            return Ok(data);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var data = await _repo.GetAllUsers();
            return Ok(data);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserById([FromQuery] int Id)
        {
            var data = await _repo.GetUserById(Id);
            return Ok(data);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            var data = await _repo.UpdateUser(user);
            return Ok(data);
        }

        [HttpDelete]
        //[Authorize]
        public async Task<IActionResult> DeleteUser([FromBody] int Id)
        {
            var data = await _repo.DeleteUser(Id);
            return Ok(data);
        }

    }
}
