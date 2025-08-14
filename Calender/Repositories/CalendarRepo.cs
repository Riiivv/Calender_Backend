using Calender.Interface;
using Calender.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calender.Repositories
{
    public class CalendarRepo : ICalendar
    {
        private readonly DatabaseContext ctx;

        public CalendarRepo(DatabaseContext context)
        {
            ctx = context;
        }

        public async Task<List<Calendar>> GetCalendarsAsync()
        {
            return await ctx.Calendars
                .Include(c => c.Events)
                .Include(c => c.CalendarUsers)
                .Include(c => c.Invitations)
                .ToListAsync();
        }

        public async Task<Calendar?> GetCalendarByIdAsync(int id)
        {
            return await ctx.Calendars
                .Include(c => c.Events)
                .Include(c => c.CalendarUsers)
                .Include(c => c.Invitations)
                .FirstOrDefaultAsync(c => c.CalendarId == id);
        }

        public async Task<int> AddCalendarAsync(Calendar calendar)
        {
            ctx.Calendars.Add(calendar);
            await ctx.SaveChangesAsync();
            return calendar.CalendarId;
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await ctx.Users.AnyAsync(u => u.UserId == userId);
        }

        public async Task UpdateCalendarAsync(Calendar calendar)
        {

            var existingCalendar = await ctx.Calendars.FindAsync(calendar.CalendarId);
            if (existingCalendar == null)
                throw new KeyNotFoundException($"Calendar with ID {calendar.CalendarId} not found.");

            existingCalendar.CalendarName = calendar.CalendarName;
            existingCalendar.Userid = calendar.Userid;

            await ctx.SaveChangesAsync();
        }

        public async Task DeleteCalendarAsync(int id)
        {
            var calendar = await ctx.Calendars
                .Include(c => c.Events)
                .Include(c => c.CalendarUsers)
                .Include(c => c.Invitations)
                .FirstOrDefaultAsync(c => c.CalendarId == id);

            if (calendar == null)
                throw new KeyNotFoundException("Calendar not found");

            ctx.Calendars.Remove(calendar);
            await ctx.SaveChangesAsync();
        }
    }
}
