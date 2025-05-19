using System.Text.Json.Serialization;

namespace Calender.Models
{
    public class Event
    {
        [JsonIgnore]
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public string? EventDescription { get; set; } 
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }

        public virtual int CalendarId { get; set; }
        [JsonIgnore]
        public Calendar? Calendar { get; set; }
        [JsonIgnore]
        public virtual List<EventUser>? EventUsers { get; set; } = new List<EventUser>();
        [JsonIgnore]
        public virtual List<EventInvitation>? Invitations { get; set; } = new List<EventInvitation>();
    }
}
