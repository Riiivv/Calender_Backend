using System.Text.Json.Serialization;

namespace Calender.Models
{
    public class User
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        [JsonIgnore]
        public virtual List<Calendar>? Calendars { get; set; } = new List<Calendar>();

        [JsonIgnore]
        public virtual List<CalendarInvitation>? SentInvitations { get; set; } = new List<CalendarInvitation>();

        [JsonIgnore]
        public virtual List<CalendarInvitation>? RecievedInvitations { get; set; } = new List<CalendarInvitation>();

        [JsonIgnore]
        public virtual List<EventInvitation>? SentEventInvitations { get; set; } = new List<EventInvitation>();

        [JsonIgnore]
        public virtual List<EventInvitation>? RecievedEventInvitations { get; set; } = new List<EventInvitation>();

        [JsonIgnore]
        public virtual List<EventUser>? EventUsers { get; set; } = new List<EventUser>();

        [JsonIgnore]
        public virtual List<CalendarUser>? CalendarUsers { get; set; } = new List<CalendarUser>();
    }
}
