using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Server.Hubs
{
    public class TestHub : Hub
    {
        public string Echo(string message)
        {
            return message;
        }
    }
}