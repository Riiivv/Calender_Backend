using Calender.Interface;
using Calender.Models;

namespace Calender.Repositories
{
    public class CalendarInvitationRepo : ICalendarInvitaiton
    {
        DatabaseContext ctx;

        public CalendarInvitationRepo(DatabaseContext context)
        {
            ctx = context;
        }

        public List<CalendarInvitation> GetCalendars()
        {
            return ctx.CalendarInvitations.ToList();
        }
        public User GetRecipientByCalendarInvitaiton(int invitationId)
        {
            int RecipientId = ctx.CalendarInvitations.Where(i => i.InvitationId == invitationId).Select(r => r.RecipientId).FirstOrDefault();
            User user = ctx.Users.Where(u => u.UserId == RecipientId).FirstOrDefault();
            return user;
        }
        public User GetSenderByCalenderInvitation(int invitationId)
        {
            int SenderId = ctx.CalendarInvitations.Where(i => i.InvitationId == invitationId).Select(s => s.SenderId).FirstOrDefault();
            User user = ctx.Users.Where(u => u.UserId ==  SenderId).FirstOrDefault();
            return user;
        }
    }
}
// calender id > modtager id = return user;
//Find User fra RecipientId