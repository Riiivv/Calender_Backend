using System.Text.Json.Serialization;

namespace Calender.Models
{
    public class EventInvitation
    {
        public int InvitationId { get; set; }  // Sørg for, at denne findes og er stavet korrekt
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public int EventId { get; set; }
        [JsonIgnore]
        public virtual User? Sender { get; set; }
        [JsonIgnore]
        public virtual User? Recipient { get; set; }
        [JsonIgnore]
        public virtual Event? Event { get; set; }
    }
}
