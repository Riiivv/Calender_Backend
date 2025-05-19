using System.Text.Json.Serialization;

namespace Calender.Models
{
    public class EventUser
    {
        public int UserId { get; set; }
        [JsonIgnore]
        public virtual Event? Event { get; set; }
        public int EventId { get; set; }
        [JsonIgnore]
        public virtual User? User { get; set; }
        public int Permissions { get; set; }
    }
}
