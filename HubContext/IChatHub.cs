namespace ChatApp.HubContext
{
    public interface IChatHub
    {
        Task RecieveMessage (string message);
    }
}
