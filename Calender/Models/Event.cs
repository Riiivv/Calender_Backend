namespace Calender.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public int CalendarId { get; set; }
        public string EventTitle { get; set; }
        public string? EventDescription { get; set; } 
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }

    }
}
