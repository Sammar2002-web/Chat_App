using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public int? GroupId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public MessageStatus Status { get; set; } = MessageStatus.Sent;
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string? Url { get; set; }

        [ForeignKey("SenderId")]
        public User? Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public User? Receiver { get; set; }
        public Group? Group { get; set; }
    }

    public enum MessageStatus
    {
        Sent,
        Delivered,
        Read
    }

    public enum MessageType
    {
        Text,
        Image,
        Video,
        Audio,
        File
    }
}
