namespace ChatApp.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;

        public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    }
}
