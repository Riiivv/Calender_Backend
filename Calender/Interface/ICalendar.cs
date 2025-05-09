using Calender.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Interface
{
    public interface ICalendar
    {
        Task<List<Calendar>> GetCalendarsAsync();
        Task<Calendar?> GetCalendarByIdAsync(int id);
        Task AddCalendarAsync(Calendar calendar);
        Task UpdateCalendarAsync(Calendar calendar);
        Task DeleteCalendarAsync(int id);
    }
}
