using Calender.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Interface
{
    public interface IEventUser
    {
        Task<List<EventUser>> GetAllEventUsersAsync();
        Task<EventUser?> GetEventUserAsync(int eventId, int userId);
        Task AddEventUserAsync(EventUser eventUser);
        Task UpdateEventUserAsync(EventUser eventUser);
        Task DeleteEventUserAsync(int eventId, int userId);
    }
}
