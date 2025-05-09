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
            return await ctx.Calendars.ToListAsync();
        }

        public async Task<Calendar?> GetCalendarByIdAsync(int id)
        {
            return await ctx.Calendars.FindAsync(id);
        }

        public async Task AddCalendarAsync(Calendar calendar)
        {
            ctx.Calendars.Add(calendar);
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateCalendarAsync(Calendar calendar)
        {
            ctx.Calendars.Update(calendar);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteCalendarAsync(int id)
        {
            var calendar = await ctx.Calendars.FindAsync(id);
            if (calendar != null)
            {
                ctx.Calendars.Remove(calendar);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
