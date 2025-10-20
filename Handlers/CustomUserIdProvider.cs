using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatApp.Handlers
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("UserId")?.Value;
        }
    }
}
