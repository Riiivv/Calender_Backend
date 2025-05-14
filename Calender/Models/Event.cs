namespace Calender.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public string? EventDescription { get; set; } 
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }


        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; }

        public List<EventUser> EventUsers { get; set; } = new List<EventUser>();
        public List<EventInvitation> Invitations { get; set; } = new List<EventInvitation>();
    }
}
