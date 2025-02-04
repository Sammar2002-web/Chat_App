using ChatApp.ApplicationDbContext;
using ChatApp.Handlers;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAppLogsRepository _logsRepository;

        public UserRepository(AppDbContext context, IConfiguration configuration, IHttpContextAccessor contextAccessor, IAppLogsRepository logsRepository)
        {
            _context = context;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _logsRepository = logsRepository;
        }

        public async Task<BaseResult> Login(LoginDto login)
        {
            try
            {
                var user = _context.users.FirstOrDefault(x => x.Password == login.Password && x.Email == login.Email);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(_configuration["Jwt:Key"]) ||
                        string.IsNullOrEmpty(_configuration["Jwt:Issuer"]) ||
                        string.IsNullOrEmpty(_configuration["Jwt:Audience"]))
                    {
                        return new BaseResult
                        {
                            IsError = true,
                            Message = "JWT configuration is missing",
                            Data = null
                        };
                    }

                    var claims = new[]
                    {
                      new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]!),
                      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("D")),
                      new Claim("UserId", user.Id.ToString()),
                      new Claim("Email", user.Email.ToString())
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var tokenExpiration = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "20");
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddMinutes(tokenExpiration),
                        signingCredentials: signIn);

                    string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

                    _contextAccessor.HttpContext.Session.SetString(GlobalConfig.LoginSessionName, user.Id.ToString());
                    var id = _contextAccessor.HttpContext.Session.GetString(GlobalConfig.LoginSessionName);

                    if (user.DateTime == null)
                    {
                        return new BaseResult
                        {
                            IsError = true,
                            Message = "User date information is missing",
                            Data = null
                        };
                    }

                    _contextAccessor.HttpContext.Response.Cookies.Append(GlobalConfig.LoginCookieName, user.Id.ToString(),
                    new CookieOptions
                    {
                        IsEssential = true,
                        Expires = user.DateTime.AddDays(20),
                        HttpOnly = true
                    });

                    return new BaseResult
                    {
                        IsError = false,
                        Message = "Login successful",
                        Data = new { Token = tokenValue, User = user }
                    };
                }

                return new BaseResult
                {
                    IsError = true,
                    Message = "Invalid email or password",
                    Data = null
                };
            }
            catch (Exception)
            {
                return new BaseResult
                {
                    IsError = true,
                    Code = System.Net.HttpStatusCode.InternalServerError,
                    Data = null
                };
            }
        }


        public async Task<BaseResult> AddUser(User user)
        {
            try
            {
                var data = await _context.users.AddAsync(user);
                await _context.SaveChangesAsync();

                return new BaseResult
                {
                    IsError = false,
                    Message = "User added successfully.",
                    Data = user
                };
            }
            catch (Exception)
            {
                return new BaseResult
                {
                    IsError = true,
                    Code = System.Net.HttpStatusCode.InternalServerError,
                    Data = null
                };
            }
        }

        public async Task<BaseResult> DeleteUser(int Id)
        {
            try
            {
                var currentUser = _contextAccessor.HttpContext?.User?.FindFirst(c => c.Type.Contains("Email"))?.Value;
                var utcNow = DateTime.UtcNow;
                var localZone = TimeZoneInfo.FindSystemTimeZoneById("PST");
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);

                var user = await _context.users.FirstOrDefaultAsync(u => u.Id == Id);
                if (user == null)
                {
                    return new BaseResult
                    {
                        IsError = true,
                        Code = System.Net.HttpStatusCode.NotFound,
                        Message = "User not found"
                    };
                }

                _context.users.Remove(user);
                await _context.SaveChangesAsync();

                
                var log = await _logsRepository.AddLogs(new AppLogs
                {
                    UserName = currentUser,
                    Date = localTime,
                    Description = $"{currentUser} Deleted user {user.Name}"
                });

                return new BaseResult
                {
                    IsError = false,
                    Code = System.Net.HttpStatusCode.OK,
                    Message = "User deleted successfully"
                };
            }
            catch (Exception)
            {
                return new BaseResult
                {
                    IsError = true,
                    Code = System.Net.HttpStatusCode.InternalServerError,
                    Data = null
                };
            }
        }

        public async Task<BaseResult> GetAllUsers()
        {
            try
            {
                var data = await _context.users.ToListAsync();
                await _context.SaveChangesAsync();

                return new BaseResult
                {
                    IsError = false,
                    Code = System.Net.HttpStatusCode.OK,
                    Data = data
                };
            }
            catch (Exception)
            {
                return new BaseResult
                {
                    IsError = true,
                    Code = System.Net.HttpStatusCode.InternalServerError,
                    Data = null
                };
            }
        }

        public async Task<BaseResult> GetUserById(int Id)
        {
            try
            {
                var data = await _context.users.FirstOrDefaultAsync(x => x.Id == Id);
                if (data == null)
                {
                    return new BaseResult
                    {
                        IsError = true,
                        Code = System.Net.HttpStatusCode.NotFound,
                        Message = "User not found"
                    };


                }

                await _context.SaveChangesAsync();
                return new BaseResult
                {
                    IsError = false,
                    Code = System.Net.HttpStatusCode.OK,
                    Data = data
                };
            }
            catch (Exception)
            {
                return new BaseResult
                {
                    IsError = true,
                    Code = System.Net.HttpStatusCode.InternalServerError,
                    Data = null
                };
            }
        }

        public async Task<BaseResult> UpdateUser(User updatedUser)
        {
            try
            {
                var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Id == updatedUser.Id);
                if (existingUser == null)
                {
                    return new BaseResult
                    {
                        IsError = true,
                        Code = System.Net.HttpStatusCode.NotFound,
                        Message = "User not found"
                    };
                }

                existingUser.Name = updatedUser.Name;
                existingUser.Email = updatedUser.Email;
                existingUser.Password = updatedUser.Password;

                await _context.SaveChangesAsync();

                var updatedUserDto = new
                {
                    existingUser.Id,
                    existingUser.Name,
                    existingUser.Email,
                    existingUser.Password
                };

                var currentUser = _contextAccessor.HttpContext?.User?.FindFirst(c => c.Type.Contains("Email"))?.Value;
                var utcNow = DateTime.UtcNow;
                var localZone = TimeZoneInfo.FindSystemTimeZoneById("PST");
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);

                var log = await _logsRepository.AddLogs(new AppLogs
                {
                    UserName = currentUser,
                    Date = localTime,
                    Description = $"{currentUser} Updated user {updatedUser.Name}"
                });

                return new BaseResult
                {
                    IsError = false,
                    Code = System.Net.HttpStatusCode.OK,
                    Data = updatedUserDto,
                    Message = "User updated successfully"
                };
            }
            catch (Exception)
            {
                return new BaseResult
                {
                    IsError = true,
                    Code = System.Net.HttpStatusCode.InternalServerError,
                    Data = null
                };
            }
        }



    }
}
