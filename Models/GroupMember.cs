namespace ChatApp.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int GroupId { get; set; } = -1;
        public int UserId { get; set; } = -1;

        public Group Group { get; set; } = new Group();
        public User User { get; set; } = new User();
    }
}
