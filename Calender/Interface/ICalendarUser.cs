using Calender.Models;

namespace Calender.Interface
{
    public interface ICalendarUser
    {
        Task<List<CalendarUser>> GetAllCalendarUsersAsync();
        Task<CalendarUser?> GetCalendarUserAsync(int calendarId, int userId);
        Task AddCalendarUserAsync(CalendarUser calendarUser);
        Task UpdateCalendarUserAsync(CalendarUser calendarUser);
        Task DeleteCalendarUserAsync(int calendarId, int userId);
    }
}
