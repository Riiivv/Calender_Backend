using System.Text.Json.Serialization;

namespace Calender.Models
{
    public class CalendarUser
    {
        public int CalendarId { get; set; }
        [JsonIgnore]
        public virtual Calendar? Calendar { get; set; }

        public int UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }

        public int Permissions { get; set; }
    }
}
