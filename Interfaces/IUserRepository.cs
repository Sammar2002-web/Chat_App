using ChatApp.Handlers;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Interfaces
{
    public interface IUserRepository
    {
        Task<BaseResult> AddUser(User user);
        Task<BaseResult> Login(LoginDto login);
        Task<BaseResult> GetAllUsers();
        Task<BaseResult> GetUserById(int Id);
        Task<BaseResult> UpdateUser(User user);
        Task<BaseResult> DeleteUser(int Id);
    }
}
