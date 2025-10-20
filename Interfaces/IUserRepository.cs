using ChatApp.Handlers;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Interfaces
{
    public interface IUserRepository
    {
        Task<BaseResult> GetById(int id);
        Task<BaseResult> GetAll();
        Task<BaseResult> Create(User user);
        Task<BaseResult> Update(User user);
        Task<BaseResult> Delete(int id);
        Task<User> GetByEmail(string email);
        Task<BaseResult> UpdateProfile(int id, UserDto userDto);
        Task<BaseResult> Login(LoginDto login);
    }
}
