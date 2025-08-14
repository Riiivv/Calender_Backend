using Calender.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Interface
{
    public interface IEvent
    {
        Task<List<Event>> GetAllUserEventsAsync(int userId);
        Task<Event?> GetEventByIdAsync(int eventId);
        Task AddEventAsync(Event eevent);
        Task UpdateEventAsync(Event eevent);
        Task DeleteEventAsync(int eventId);
    }
}
