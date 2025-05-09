namespace Calender.Models
{
    public class CalendarInvitation
    {
        public int InvitationId { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public int CalendarId { get; set; }
    }
}
