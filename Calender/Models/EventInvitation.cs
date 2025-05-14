namespace Calender.Models
{
    public class EventInvitation
    {
        public int InvitationId { get; set; }  // Sørg for, at denne findes og er stavet korrekt
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public int EventId { get; set; }

        public User Sender { get; set; }
        public User Recipient { get; set; }
        public Event Event { get; set; }
    }
}
