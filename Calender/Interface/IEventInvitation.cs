using Calender.Models;

namespace Calender.Interface
{
    public interface IEventInvitation
    {
        Task<List<EventInvitation>> GetAllEventInvitationsAsync();
        Task<EventInvitation?> GetEventInvitationAsync(int eventId, int recipientId);
        Task AddEventInvitationAsync(EventInvitation invitation);
        Task UpdateEventInvitationAsync(EventInvitation invitation);
        Task DeleteEventInvitationAsync(int eventId, int recipientId);
    }
}
