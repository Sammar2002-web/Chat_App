using System.Text.Json.Serialization;

namespace ChatApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime DateTime { get; set; } = DateTime.Now;

        public ICollection<Message>? SentMessages { get; set; } = new List<Message>();
        public ICollection<Message>? ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<GroupMember>? GroupMemberships { get; set; } = new List<GroupMember>();
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
