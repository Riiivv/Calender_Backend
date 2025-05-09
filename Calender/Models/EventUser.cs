namespace Calender.Models
{
    public class EventUser
    {
        public int UserId { get; set; }
        public int EventId { get; set; }
        public int Permissions { get; set; }
    }
}
