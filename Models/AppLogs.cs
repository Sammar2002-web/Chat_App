namespace ChatApp.Models
{
    public class AppLogs
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date {  get; set; } = DateTime.Now;
    }
}
