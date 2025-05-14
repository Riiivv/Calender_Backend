using Calender.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Interface
{
    public interface ICalendarInvitation
    {
        Task<List<CalendarInvitation>> GetAllCalendarInvitationsAsync();
        Task<CalendarInvitation?> GetCalendarInvitationAsync(int invitationId);
        Task AddCalendarInvitationAsync(CalendarInvitation invitation);
        Task UpdateCalendarInvitationAsync(CalendarInvitation invitation);
        Task DeleteCalendarInvitationAsync(int invitationId);
        Task<User?> GetRecipientByCalendarInvitationAsync(int invitationId);
        Task<User?> GetSenderByCalendarInvitationAsync(int invitationId);
    }
}
