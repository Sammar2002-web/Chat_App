using ChatApp.ApplicationDbContext;
using ChatApp.Handlers;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<BaseResult> GetById(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return new BaseResult { IsError = true, Message = "User not found." };
            }
            return new BaseResult { Data = user };
        }

        public async Task<BaseResult> GetAll()
        {
            var users = await _context.users.ToListAsync();
            return new BaseResult { Data = users };
        }

        public async Task<BaseResult> Create(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _context.users.AddAsync(user);
            await _context.SaveChangesAsync();
            return new BaseResult { Data = user };
        }

        public async Task<BaseResult> Update(User user)
        {
            _context.users.Update(user);
            await _context.SaveChangesAsync();
            return new BaseResult { Data = user };
        }

        public async Task<BaseResult> Delete(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return new BaseResult { IsError = true, Message = "User not found." };
            }
            _context.users.Remove(user);
            await _context.SaveChangesAsync();
            return new BaseResult { Message = "User deleted successfully." };
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _context.users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<BaseResult> UpdateProfile(int id, UserDto userDto)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return new BaseResult { IsError = true, Message = "User not found." };
            }

            user.Name = userDto.Name;
            user.Email = userDto.Email;

            await _context.SaveChangesAsync();

            return new BaseResult { IsError = false, Message = "Profile updated successfully." };
        }

        public async Task<BaseResult> Login(LoginDto login)
        {
            var user = await _context.users.FirstOrDefaultAsync(x => x.Email == login.Email);
            if (user != null && BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Email", user.Email)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: signIn);

                string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

                return new BaseResult
                {
                    IsError = false,
                    Message = "Login successful",
                    Data = new { Token = tokenValue, User = new { user.Id, user.Name, user.Email } }
                };
            }

            return new BaseResult
            {
                IsError = true,
                Message = "Invalid email or password",
                Data = null
            };
        }
    }
}