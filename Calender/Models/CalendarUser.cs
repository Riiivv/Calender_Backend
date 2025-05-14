namespace Calender.Models
{
    public class CalendarUser
    {
        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int Permissions { get; set; }
    }
}
