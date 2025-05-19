using System.Text.Json.Serialization;

namespace Calender.Models
{
    public class Calendar
    {
        [JsonIgnore]
        public int CalendarId { get; set; }
        public string CalendarName { get; set; }
        public int Userid { get; set; }

        [JsonIgnore]
        public virtual List<Event>? Events { get; set; } = new List<Event>();
        [JsonIgnore]
        public virtual List<CalendarInvitation>? Invitations { get; set; } = new List<CalendarInvitation>();
        [JsonIgnore]
        public virtual List<CalendarUser>? CalendarUsers { get; set; } = new List<CalendarUser>();
        [JsonIgnore]
        public virtual User? User { get; set; }

    }
}
