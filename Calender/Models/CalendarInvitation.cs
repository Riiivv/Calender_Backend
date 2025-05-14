namespace Calender.Models
{
    public class CalendarInvitation
    {
        public int InvitationId { get; set; }  // Primary Key
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public int CalendarId { get; set; }

        public User Sender { get; set; }
        public User Recipient { get; set; }
        public Calendar Calendar { get; set; }
    }
}
