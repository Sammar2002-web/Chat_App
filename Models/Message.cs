namespace ChatApp.Models

{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; } = -1;
        public int ReceiverId { get; set; } = -1;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; } = string.Empty;
    }
}
