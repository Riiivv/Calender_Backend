namespace Calender.Models
{
    public class Calendar
    {
        public int CalendarId { get; set; }
        public string CalendarName { get; set; }
        public int Userid { get; set; }

        public List<Event> Events { get; set; } = new List<Event>();
        public List<CalendarInvitation> Invitations { get; set; } = new List<CalendarInvitation>();
        public List<CalendarUser> CalendarUsers { get; set; } = new List<CalendarUser>();

        public User User { get; set; }

    }
}
