using ChatApp.ApplicationDbContext;
using ChatApp.Handlers;
using ChatApp.Interfaces;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Repositories
{
    public class AppLogsRepository : IAppLogsRepository
    {
        private readonly AppDbContext _context;

        public AppLogsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResult> AddLogs(AppLogs logs)
        {
            try
            {
                var data = await _context.AddAsync(logs);
                await _context.SaveChangesAsync();

                return new BaseResult
                {
                    Data = data.Entity,
                    Code = System.Net.HttpStatusCode.OK,
                    IsError = false,
                    Message = "Log added successfully"
                };
            }
            catch (Exception)
            {
                return new BaseResult
                {
                    Code = System.Net.HttpStatusCode.InternalServerError,
                    IsError = false,
                    Data = null
                };
            }
        }

        public async Task<BaseResult> GetLogs()
        {
            try
            {
                var data = await _context.appLogs.ToListAsync();
                await _context.SaveChangesAsync();

                return new BaseResult
                {
                    Data = data,
                    Code = System.Net.HttpStatusCode.OK,
                    IsError = false
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
