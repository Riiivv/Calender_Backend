using System.Text.Json.Serialization;

namespace Calender.Models
{
    public class CalendarInvitation
    {
        [JsonIgnore]
        public int InvitationId { get; set; }  // Primary Key
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public int CalendarId { get; set; }

        [JsonIgnore]
        public virtual User? Sender { get; set; }
        [JsonIgnore]
        public virtual User? Recipient { get; set; }
        [JsonIgnore]
        public virtual Calendar? Calendar { get; set; }
    }
}
