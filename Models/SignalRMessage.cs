namespace ChatApp.Models
{
    public class SignalRMessage
    {
        public int Id { get; set; }
        public int SenderId { get; set; } = 0;
        public int ReceiverId { get; set; } = 0;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

}